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
        public List<RenderData> RenderDatas;
        public override void Start()
        {
            RenderDatas = new List<RenderData>();
        }

        public override void Update()
        {
                MainWindow.MainRenderer.AddObject(this);
        }

        public Matrix4x4 CalculateObjectTransformMatrix()
        {
            return TransformMatrixCaculator.CreateTranslationMatrix(Controller.WorldPosition) * TransformMatrixCaculator.CreateRotationMatrix(Controller.WorldRotation);
        }
        public Matrix4x4 CalculateObjectRotationMatrix()
        {
            return TransformMatrixCaculator.CreateRotationMatrix(Controller.WorldRotation);
        }
    }
}
