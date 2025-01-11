using System;
using System.Collections.Generic;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using Renderer;
using Renderer.Core;
using Renderer.Maths;
using ILGPU.Algorithms;
using NPhotoshop.Core;
using NPhotoshop;
using NPhotoshop.Core.Image;
using ILGPU.Runtime.OpenCL;
using System.Threading.Tasks;

public class GPURasterizer : IDisposable
{
    // 기존 상수
    const int tileSize = 2;
    const int MaxTCount = 64;

    // ILGPU 관련 필드
    private Context context;
    private Accelerator accelerator;

    // GPU로 돌릴 커널(함수 포인터)
    private Action<Index1D, ArrayView<Vertex>, int, int> Kernel_ConvertVertexToScreenSpaceKernel;
    private Action<Index1D,
                   ArrayView<int>,
                   ArrayView<int>,
                   ArrayView<Vertex>,
                   ArrayView<int>,
                   int,
                   int,
                   int,
                   int> Kernel_CacheTrianglesPerTile;
    private Action<Index1D,
                   ArrayView<float>,
                   ArrayView<Vertex>,
                   ArrayView<int>,
                   ArrayView<int>,
                   ArrayView<int>,
                   ArrayView<Raster>,
                   int,
                   int,
                   int,
                   int> Kernel_CalculateRastersPerTile;

    // CPU 측 배열
    int[] TriangleIndices_PerTile; //value : triangleIndex
    int[] TriangleCount_PerTile;   //value : count of triangles
    Raster[] Rasters;
    int Width;
    int Height;
    float[] ZBuffer;
    MemoryBuffer1D<int, Stride1D.Dense> devTriangleIndices_PerTile;
    MemoryBuffer1D<int, Stride1D.Dense> devTriangleCount_PerTile;
    MemoryBuffer1D<float, Stride1D.Dense> devZBuffer;
    MemoryBuffer1D<Raster, Stride1D.Dense> devRasters;

    public GPURasterizer(int width, int height)
    {
        Width = width;
        Height = height;

        int widthInTiles = width / tileSize;
        int heightInTiles = height / tileSize;
        int numTiles = widthInTiles * heightInTiles;

        Rasters = new Raster[width * height];
        TriangleCount_PerTile = new int[widthInTiles * heightInTiles];
        TriangleIndices_PerTile = new int[widthInTiles * heightInTiles * MaxTCount];
        ZBuffer = new float[width * height];
        // ILGPU Context/Accelerator 생성
        context = Context.CreateDefault();
        accelerator = context.CreateCudaAccelerator(0);

        // 필요한 커널 로드
        Kernel_ConvertVertexToScreenSpaceKernel = accelerator.LoadAutoGroupedStreamKernel
            <Index1D, ArrayView<Vertex>, int, int>
            (GPURasterizerInternal.Kernel_ConvertVertexToScreenSpace);

        Kernel_CacheTrianglesPerTile = accelerator.LoadAutoGroupedStreamKernel
            <Index1D,
             ArrayView<int>,  // TriangleIndices_PerTile
             ArrayView<int>,  // TriangleCount_PerTile
             ArrayView<Vertex>,
             ArrayView<int>,
             int,
             int,
             int,
             int>
            (GPURasterizerInternal.CalculateCache_TrianglesOnPerTileKernel);

        Kernel_CalculateRastersPerTile = accelerator.LoadAutoGroupedStreamKernel
            <Index1D,
             ArrayView<float>,
             ArrayView<Vertex>,
             ArrayView<int>,
             ArrayView<int>,
             ArrayView<int>,
             ArrayView<Raster>,
             int,
             int,
             int,
             int>
            (GPURasterizerInternal.CalculateRasters_PerTileKernel);

        devTriangleIndices_PerTile = accelerator.Allocate1D<int>(TriangleIndices_PerTile.Length);
        devTriangleCount_PerTile = accelerator.Allocate1D<int>(TriangleCount_PerTile.Length);
        devZBuffer = accelerator.Allocate1D<float>(ZBuffer.Length);
        devRasters = accelerator.Allocate1D<Raster>(Rasters.Length);
        devRasters.CopyFromCPU(Rasters);
    }


    public void Start()
    {
        Parallel.For(0, ZBuffer.Length, (i) =>
        {
            ZBuffer[i] = float.MaxValue;
        });
        devZBuffer.CopyFromCPU(ZBuffer);
    }

    private void InitializeTriangleCacheData()
    {
        Parallel.For(0, Rasters.Length, (i) =>
        {
            Rasters[i].TriangleIndex = -1;
        });

        devTriangleIndices_PerTile.CopyFromCPU(TriangleIndices_PerTile);
        devTriangleCount_PerTile.CopyFromCPU(TriangleCount_PerTile);
    }
    private void InitializeRasterData()
    {
        for (int i = 0; i < Rasters.Length; i++)
        {
            Rasters[i].TriangleIndex = -1;
        }
        devRasters.CopyFromCPU(Rasters);
    }

    public Raster[] Run(Vertex[] vertices, NBitmap target, int[] triangles, int width, int height)
    {

        //클리핑
        (vertices, triangles) = GPURasterizerInternal.ClipTriangles(vertices, triangles);

        if (vertices.Length == 0)
            return null;

        InitializeTriangleCacheData();
        InitializeRasterData();

        using var devVertices = accelerator.Allocate1D<Vertex>(vertices.Length);
        devVertices.CopyFromCPU(vertices);

        using var devTriangles = accelerator.Allocate1D<int>(triangles.Length);
        devTriangles.CopyFromCPU(triangles);


        Kernel_ConvertVertexToScreenSpaceKernel(
            vertices.Length,
            devVertices.View,
            width,
            height
        );
        accelerator.Synchronize();

        Kernel_CacheTrianglesPerTile(
            triangles.Length / 3,
            devTriangleIndices_PerTile.View,
            devTriangleCount_PerTile.View,
            devVertices.View,
            devTriangles.View,
            width,
            height,
            tileSize,
            MaxTCount
        );
        accelerator.Synchronize();
        int widthInTiles = width / tileSize;
        int heightInTiles = height / tileSize;
        int numTiles = widthInTiles * heightInTiles;
        Kernel_CalculateRastersPerTile(
            numTiles,
            devZBuffer.View,
            devVertices.View,
            devTriangles.View,
            devTriangleIndices_PerTile.View,
            devTriangleCount_PerTile.View,
            devRasters.View,
            width,
            height,
            tileSize,
            MaxTCount
        );
        accelerator.Synchronize();

        //devVertices.CopyToCPU(vertices);

        //뭐지 이거 하면 에러뜸(아마 IndexOutOfRange?)
        //devTriangleIndices_PerTile.CopyToCPU(TriangleIndices_PerTile);
        //devTriangleCount_PerTile.CopyToCPU(TriangleCount_PerTile);
        devRasters.CopyToCPU(Rasters);

        return Rasters;
    }


    public void Dispose()
    {
        accelerator?.Dispose();
        context?.Dispose();
    }
}
