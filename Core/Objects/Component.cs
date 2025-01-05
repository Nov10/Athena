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
        bool IsStarted;
        public void Initialize(Object controller)
        {
            Controller = controller;
        }
        public Component()
        {
            Awake();
            //Start();
        }
        public abstract void Awake();
        public void UpdateComponent()
        {
            if(IsStarted == false)
            {
                Start();
                IsStarted = true;
            }
            else
            {
                Update();
            }
        }
        public abstract void Start();
        public abstract void Update();
    }
}
