using System;
using System.Collections.Generic;
using System.Linq;
using Renderer.Maths;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Core
{
    public class Object
    {
        public Object Parent;
        public List<Object> Children;
        public Vector3 LocalPosition;
        public Vector3 LocalRotation;

        public Vector3 WorldPosition
        {
            get
            {
                if (Parent == null)
                    return LocalPosition;

                Vector3 worldParentPosition = Parent.WorldPosition;
                Vector3 worldParentRotation = Parent.WorldRotation;

               return worldParentPosition + TransformMatrixCaculator.Transform(LocalPosition, TransformMatrixCaculator.CreateRotationMatrix(worldParentRotation));
            }
        }

        public Vector3 WorldRotation
        {
            get
            {
                if (Parent == null)
                    return LocalRotation;
                return Parent.WorldRotation + LocalRotation;
            }
        }

        public List<Component> Components;

        public void AddComponent(Component component)
        {
            Components.Add(component);
            component.Initialize(this);
        }

        public virtual void Update()
        {
            for(int i = 0; i < Components.Count; i++)
            {
                Components[i].Update();
            }
        }

        public Object()
        {
            Components = new List<Component>();
            Children = new List<Object>();
            Parent = null;
        }
    }
}
