using Renderer.Maths;
using Renderer.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Core
{
    public class Renderer : Component
    {
        public List<RenderObject> Objects;
        public override void Start()
        {
            Objects = new List<RenderObject>();
        }

        public override void Update()
        {
                MainWindow.MainRenderer.AddObject(this);
        }

        public Matrix4x4 CalculateObjectTransformMatrix()
        {
            return TransformMatrixCaculator.CreateTranslationMatrix(Controller.Position) * TransformMatrixCaculator.CreateRotationMatrix(Controller.Rotation);
        }
    }
}
