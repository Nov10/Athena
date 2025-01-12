using System;
using System.Collections.Generic;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using Athena.Maths;
using ILGPU.Algorithms;
using System.Threading.Tasks;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering;
using Renderer.Engine.Core.Rendering;

namespace Athena.Engine.Core.Rendering
{
    public partial class GPURasterizer
    {
        // 기존 상수
        const int tileSize = 4;
        const int MaxTCount = 64;

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
        private Action<Index1D, ArrayView<float>> Kernel_ClearZBuffer;
        private Action<Index1D, ArrayView<Raster>> Kernel_ClearRasters;
        private Action<Index1D, ArrayView<int>> Kernel_ClearTriangleCache;

        Raster[] Rasters;
        int Width;
        int Height;
        int TileCount;
        int PixelCount;

        MemoryBuffer1D<int, Stride1D.Dense> devTriangleIndices_PerTile;
        MemoryBuffer1D<int, Stride1D.Dense> devTriangleCount_PerTile;
        MemoryBuffer1D<float, Stride1D.Dense> devZBuffer;
        MemoryBuffer1D<Raster, Stride1D.Dense> devRasters;
        MemoryBuffer1D<Color, Stride1D.Dense> devFrameBuffer;


        public GPURasterizer(int width, int height)
        {
            Width = width;
            Height = height;
            PixelCount = width * height;

            int widthInTiles = width / tileSize;
            int heightInTiles = height / tileSize;
            TileCount = widthInTiles * heightInTiles;

            Kernel_ConvertVertexToScreenSpaceKernel = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Vertex>, int, int>
                (InternelKernel_ConvertVertexToScreenSpace);

            Kernel_CacheTrianglesPerTile = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D,
                 ArrayView<int>,
                 ArrayView<int>,
                 ArrayView<Vertex>,
                 ArrayView<int>,
                 int,
                 int,
                 int,
                 int>
                (InternalKernel_CalculateCacheTrianglesPerTile);

            Kernel_CalculateRastersPerTile = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
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
                (InternalKernel_CalculateRastersPerTile);

            Kernel_ClearZBuffer = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<float>>(ClearZBufferKernel);

            Kernel_ClearRasters = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Raster>>(ClearRastersKernel);

            Kernel_ClearTriangleCache = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<int>>(ClearTriangleCacheKernel);


            Rasters = new Raster[PixelCount];

            devTriangleIndices_PerTile = GPUAccelator.Accelerator.Allocate1D<int>(TileCount * MaxTCount);
            devTriangleCount_PerTile = GPUAccelator.Accelerator.Allocate1D<int>(TileCount);
            devZBuffer = GPUAccelator.Accelerator.Allocate1D<float>(PixelCount);
            devRasters = GPUAccelator.Accelerator.Allocate1D<Raster>(Rasters.Length);
            devFrameBuffer = GPUAccelator.Accelerator.Allocate1D<Color>(PixelCount);
            devRasters.CopyFromCPU(Rasters);
        }

        public void Start()
        {
            Kernel_ClearZBuffer(PixelCount, devZBuffer.View);
        }

        private void InitializeTriangleCacheData()
        {
            Kernel_ClearTriangleCache(TileCount, devTriangleCount_PerTile.View);
            Kernel_ClearRasters(Rasters.Length, devRasters.View);
        }

        public Raster[] Run(Vertex[] vertices, int[] triangles, int width, int height)
        {
            //클리핑
            (vertices, triangles) = ClipTriangles(vertices, triangles);

            if (vertices.Length == 0)
                return null;

            InitializeTriangleCacheData();

            using var devVertices = GPUAccelator.Accelerator.Allocate1D<Vertex>(vertices.Length);
            devVertices.CopyFromCPU(vertices);

            using var devTriangles = GPUAccelator.Accelerator.Allocate1D<int>(triangles.Length);
            devTriangles.CopyFromCPU(triangles);


            Kernel_ConvertVertexToScreenSpaceKernel(
                vertices.Length,
                devVertices.View,
                width,
                height
            );
            //accelerator.Synchronize();

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
            //accelerator.Synchronize();
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


            GPUAccelator.Accelerator.Synchronize();

            devRasters.CopyToCPU(Rasters);

            return Rasters;
        }
    }
}