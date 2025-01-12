using Athena.Engine.Core;
using Athena.Engine.Core.Image;
using Athena.Maths;
using Renderer.Engine.Core.Image;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Engine.Core.Rendering
{
    public abstract class BaseRenderer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public BaseRenderer(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public void Render(Camera camera, List<MeshRenderer> targets)
        {
            InternelRender(camera, targets);
        }

        protected abstract void InternelRender(Camera camera, List<MeshRenderer> targets);
    }
}
