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
using Athena.Engine.Core.Rendering.Shaders;
using Athena.Engine.Helpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ILGPU.Runtime.OpenCL;
using Athena.Engine.Core.Rendering.Lights;

namespace Athena.Engine.Core.Rendering
{
    /// <summary>
    /// GPU-Based Rasterizer. **이 클래스는 라이브러리에 의존합니다.**
    /// </summary>
    public partial class GPURasterizer
    {
        // 기존 상수
        const int tileSize = 4;
        const int MaxTCount = 64;

        public enum Mode
        {
            Normal = 0,
            ShadowMap=1
        }

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
        private Action<Index1D, ArrayView<Color>> Kernel_ClearFrameBuffer;
        private Action<Index1D, ArrayView<int>> Kernel_ClearTriangleCache;

        Raster[] Rasters;
        Color[] FrameBuffer;
        int Width;
        int Height;
        int TileCount;
        int PixelCount;

        MemoryBuffer1D<int, Stride1D.Dense> devTriangleIndices_PerTile;
        MemoryBuffer1D<int, Stride1D.Dense> devTriangleCount_PerTile;
        MemoryBuffer1D<float, Stride1D.Dense> devZBuffer;
        MemoryBuffer1D<Raster, Stride1D.Dense> devRasters;
        MemoryBuffer1D<Color, Stride1D.Dense> devFrameBuffer;


        void CreateNormalKernels()
        {
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
        }
        void CreateShadowMapKernels()
        {
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
                (InternalKernel_CalculateCacheTrianglesPerTile_ShadowMap);

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
                (InternalKernel_CalculateRastersPerTile_ShadowMap);
        }
        public GPURasterizer(int width, int height, Mode mode = Mode.Normal)
        {
            Width = width;
            Height = height;
            PixelCount = width * height;

            int widthInTiles = width / tileSize;
            int heightInTiles = height / tileSize;
            TileCount = widthInTiles * heightInTiles;

            switch (mode)
            {
                case Mode.Normal:
                    CreateNormalKernels();
                    break;
                case Mode.ShadowMap:
                    CreateShadowMapKernels();
                    break;
            }

            Kernel_ClearZBuffer = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<float>>(ClearZBufferKernel);

            Kernel_ClearRasters = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Raster>>(ClearRastersKernel);

            Kernel_ClearFrameBuffer = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<Color>>(ClearFrameBufferKernel);

            Kernel_ClearTriangleCache = GPUAccelator.Accelerator.LoadAutoGroupedStreamKernel
                <Index1D, ArrayView<int>>(ClearTriangleCacheKernel);


            Rasters = new Raster[PixelCount];
            FrameBuffer = new Color[PixelCount];

            devTriangleIndices_PerTile = GPUAccelator.Accelerator.Allocate1D<int>(TileCount * MaxTCount);
            devTriangleCount_PerTile = GPUAccelator.Accelerator.Allocate1D<int>(TileCount);
            devZBuffer = GPUAccelator.Accelerator.Allocate1D<float>(PixelCount);
            devRasters = GPUAccelator.Accelerator.Allocate1D<Raster>(PixelCount);
            devFrameBuffer = GPUAccelator.Accelerator.Allocate1D<Color>(PixelCount);
            devRasters.CopyFromCPU(Rasters);
        }

        public void Start()
        {
            Kernel_ClearZBuffer(PixelCount, devZBuffer.View);
            Kernel_ClearFrameBuffer(PixelCount, devFrameBuffer.View);
        }

        private void InitializeTriangleCacheData()
        {
            Kernel_ClearTriangleCache(TileCount, devTriangleCount_PerTile.View);
            Kernel_ClearRasters(Rasters.Length, devRasters.View);
        }

        public Color[] Run(MemoryBuffer1D<Vertex, Stride1D.Dense> vertices, MemoryBuffer1D<int, Stride1D.Dense> triangles, int vCount, int tCount,
            int width, int height, CustomShader shader, Light[] lightDatas, bool getFrameBuffer = true)
        {
            InitializeTriangleCacheData();

            Kernel_ConvertVertexToScreenSpaceKernel(
                (int)vCount,
                vertices.View,
                width,
                height
            );
            //accelerator.Synchronize();

            Kernel_CacheTrianglesPerTile(
                (int)tCount / 3,
                devTriangleIndices_PerTile.View,
                devTriangleCount_PerTile.View,
                vertices.View,
                triangles.View,
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
                vertices.View,
                triangles.View,
                devTriangleIndices_PerTile.View,
                devTriangleCount_PerTile.View,
                devRasters.View,
                width,
                height,
                tileSize,
                MaxTCount
            );

            shader.RunFragmentShader_GPU(devRasters, devFrameBuffer, lightDatas, width);


            GPUAccelator.Accelerator.Synchronize();
            if(getFrameBuffer == true)
            {
                devFrameBuffer.CopyToCPU(FrameBuffer);
                return FrameBuffer;
            }
            return null;
        }

        public float[] GetZBuffer()
        {
            float[] Z = new float[PixelCount];
            devZBuffer.CopyToCPU(Z);
            return Z;
        }
    }
}