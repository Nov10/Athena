using ILGPU.Runtime;
using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU.Runtime.Cuda;

namespace Renderer.Engine.Core.Rendering
{
    public class GPUAccelator
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
