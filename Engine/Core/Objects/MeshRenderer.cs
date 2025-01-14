using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Rendering;
using Athena.Engine.Core.Rendering;

namespace Athena.Engine.Core
{
    public class MeshRenderer : Component
    {
        public static List<MeshRenderer> RendererList = new List<MeshRenderer>();

        public List<RenderData> RenderDatas;
        public MeshRenderer()
        {
            RendererList.Add(this);
        }
        ~MeshRenderer()
        {
            RendererList.Remove(this);
        }
        public override void Awake()
        {
            RenderDatas = new List<RenderData>();
        }
        public override void Start()
        {
        }

        public override void Update()
        {

        }

        public Matrix4x4 CalculateObjectTransformMatrix()
        {
            return TransformMatrixCaculator.CreateObjectTransformMatrix(Controller.WorldPosition, Controller.WorldRotation, Controller.WorldScale);
            //return TransformMatrixCaculator.CreateTranslationMatrix(Controller.WorldPosition) * TransformMatrixCaculator.CreateRotationMatrix(Controller.WorldRotation.ToEulerAngles());
        }
        public Matrix4x4 CalculateObjectRotationMatrix()
        {
            return TransformMatrixCaculator.CreateRotationMatrix(Controller.WorldRotation.ToEulerAngles());
        }
    }
}
