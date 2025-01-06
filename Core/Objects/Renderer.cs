﻿using Renderer.Maths;
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
        }

        public override void Update()
        {
                //MainWindow.MainRenderer.AddObject(this);
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

        public override void Awake()
        {
            RenderDatas = new List<RenderData>();
        }
    }
}