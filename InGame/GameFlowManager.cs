using Athena.Engine.Core;
using Athena.InGame.AirPlane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Renderer.InGame
{
    public class GameFlowManager : Component
    {
        static GameFlowManager _Instance;
        public static GameFlowManager Instance
        {
            get { return _Instance; }
        }

        public Aircraft Player;
        
        public int Score { get; private set; }

        public void AddScore()
        {
            Score += 1;
            System.Diagnostics.Debug.WriteLine(Score);
        }

        public override void Awake()
        {
            _Instance = this;
        }

        public override void Start()
        {
        }

        public override void Update()
        {
        }
    }
}
