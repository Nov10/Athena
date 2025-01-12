using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Athena.Engine.Core
{
    public abstract class Component
    {
        public GameObject Controller { get; private set; }
        bool IsStarted;
        public void Initialize(GameObject controller)
        {
            Controller = controller;
        }
        public Component()
        {
            Awake();
        }
        public abstract void Awake();
        public void UpdateComponent()
        {
            if (IsStarted == false)
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
