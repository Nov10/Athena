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
        public Vector3 Position;
        public Vector3 Rotation;

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
        }
    }
}
