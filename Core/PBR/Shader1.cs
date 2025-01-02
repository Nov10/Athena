﻿using Assimp.Unmanaged;
using Microsoft.UI.Xaml.Media.Imaging;
using NPhotoshop.Core.Image;
using Renderer.Core.Shader;
using Renderer.Maths;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;

namespace Renderer.Core.PBR
{
    public class Shader1 : CustomShader
    {
        public NBitmap MainTexture;
        public NBitmap NormalTexture;
        public Shader1()
        {
            MainTexture = new NBitmap(1,1);
            NormalTexture = new NBitmap(1,1);
        }

        public override Color FragmentShader(Raster raster, Vector3 light)
        {
            //var normal = (ShaderHelper.SampleTexture(NormalTexture, raster.UV).GetAsVector3() / 255.0f).normalized;
            var normal = raster.Normal_WorldSpace;
            //normal = new Vector3(normal.z, normal.y, normal.x);
            float brightness = Vector3.Dot((normal).normalized, light);
            brightness = brightness * 0.5f + 0.5f;
            //brightness = 0.5f * brightness + 0.5f;
            //byte intensity = (byte)(brightness * 255);
            var c2 = ShaderHelper.SampleTexture(MainTexture, raster.UV) * brightness;
            
            //raster.Tangent, raster.BitTangent, raster.Normal_WorldSpace
            //var c2 = new Color(255, 255, 255, 255) * brightness;
            c2.A = 255;
            return c2;
        }

        public override Vector3 VertextShader(Vector3 vertex_position_WorldSpace, Vector3 vertex_normal_WorldSpace, Vector3 objectposition_WorldSpace)
        {
            return vertex_position_WorldSpace;
        }
    }
}
