using ILGPU;
using ILGPU.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core.Rendering
{
    public partial class RenderData
    {
        MemoryBuffer1D<Vertex, Stride1D.Dense> CachedBuffer;
        bool IsCached;
        const int CacheLastTimeInSec = 5;
        int LastUsedTime;
        public MemoryBuffer1D<Vertex, Stride1D.Dense> GetVerticesBuffer()
        {
            if(IsCached == false)
            {
                CacheBuffer();
            }
            LastUsedTime = Time.TotalTimeInt;
            return CachedBuffer;
        }

        public void UpdateCache()
        {
            if (IsCached == true)
            {
                if (Time.TotalTimeInt - LastUsedTime > CacheLastTimeInSec)
                {
                    Dispose();
                }
            }
        }


        void CacheBuffer()
        {
            CachedBuffer = GPUAccelator.Accelerator.Allocate1D<Vertex>(Vertices.Length);
            CachedBuffer.CopyFromCPU(Vertices);
            IsCached = true;
        }
        void Dispose()
        {
            CachedBuffer.Dispose();
            IsCached = false;
        }
    }
}
