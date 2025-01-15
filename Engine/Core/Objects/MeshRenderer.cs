using Athena.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Engine.Core.Rendering;

namespace Athena.Engine.Core
{
    /// <summary>
    /// Renderer for Common Mesh.
    /// </summary>
    public class MeshRenderer : Component
    {
        public static List<MeshRenderer> RendererList = new List<MeshRenderer>();

        public List<RenderData> RenderDatas { get; private set; }

        public override void Awake()
        {
            RendererList.Add(this);
            RenderDatas = new List<RenderData>();
        }
        public override void Start()
        {

        }

        public override void Update()
        {

        }
        protected override void OnDestroyed()
        {
            base.OnDestroyed();
            RendererList.Remove(this);
        }

        public Matrix4x4 CreateObjectTransformMatrix()
        {
            return TransformMatrixCaculator.CreateObjectTransformMatrix(Controller);
        }
        public Matrix4x4 CreateObjectRotationMatrix()
        {
            return TransformMatrixCaculator.CreateRotationMatrix(Controller.WorldRotation.ToEulerAngles());
        }
    }
}
