using ILGPU.Runtime;
using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.CPU;

namespace Athena.Engine.Core.Rendering
{
    /// <summary>
    /// static GPU Accelator(가속기). **이 클래스는 라이브러리에 의존합니다.**
    /// </summary>
    public static class GPUAccelator
    {
        public static Context Context { get; private set; }
        public static Accelerator Accelerator { get; private set; }


        public static void Intialize()
        {
            Context = Context.Create(builder => builder.Math(MathMode.Fast32BitOnly).Cuda());
            Accelerator = Context.CreateCudaAccelerator(0);
        }
    }
}
