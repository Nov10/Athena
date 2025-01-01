using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.Core
{
    public abstract class Component
    {
        public Object Controller;
        public void Initialize(Object controller)
        {
            Controller = controller;
        }
        public Component()
        {
            Start();
        }
        public abstract void Start();
        public abstract void Update();
    }
}
