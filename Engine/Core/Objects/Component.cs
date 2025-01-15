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
        public void InitializeComponent(GameObject controller)
        {
            Controller = controller;
        }
        public void DeInitializeComponent(GameObject controller)
        {
            Controller = null;
        }
        public Component()
        {
            Awake();
        }
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
        protected virtual void OnDestroyed()
        {

        }
        /// <summary>
        /// 초기 설정 이벤트. 생성자와 동일합니다 .
        /// </summary>
        public abstract void Awake();
        /// <summary>
        /// 초기 설정 이벤트. Awake 이후 한 번 실행됩니다.
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// 지속 이벤트. Start 이후 항상 실행됩니다.
        /// </summary>
        public abstract void Update();
    }
}
