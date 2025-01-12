using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Athena.Maths;
using Athena.Engine.Core.Image;
using Athena.Engine.Core.Rendering;
using Athena.Engine.Core;

namespace Athena.Terrain
{
    public class MapDisplay : Component
    {
        public Athena.Engine.Core.MeshRenderer RenderComponenet;

        public void DrawTexture(NBitmap texture)
        {
            (RenderComponenet.RenderDatas[0].Shader as NormalShader).MainTexture = texture;
            RenderComponenet.Controller.LocalScale = new Vector3(texture.Width, 1, texture.Height);
        }

        public void DrawMesh(MeshData meshData, NBitmap texture)
        {
            RenderComponenet.RenderDatas[0] = meshData.CreateMesh();
            (RenderComponenet.RenderDatas[0].Shader as NormalShader).MainTexture = texture;
        }

        public override void Awake()
        {
        }

        public override void Start()
        {
        }

        public override void Update()
        {
        }
    }
}
