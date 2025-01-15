using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Maths;
using System.Text;
using System.Threading.Tasks;
using Athena;
using System.Xml.Linq;

namespace Athena.Engine.Core
{
    /// <summary>
    /// Object that exists in World.
    /// </summary>
    public class GameObject
    {
        public string Name;
        public bool Active;
        List<Component> Components;

        List<GameObject> Children;
        GameObject _Parent;
        public GameObject Parent
        {
            get { return _Parent; }
            set
            {
                if(value == null)
                {
                    DisconnetParenting(_Parent, this);
                }
                //Parent 변화
                if(_Parent != value)
                {
                    DisconnetParenting(_Parent, this);
                    ConnetParenting(value, this);
                }
            }
        }
        static void DisconnetParenting(GameObject parent, GameObject child)
        {
            child._Parent = null;
            parent?.Children.Remove(child);
        }
        static void ConnetParenting(GameObject parent, GameObject child)
        {
            child._Parent = parent;
            parent.Children.Add(child);
        }

        public Vector3 LocalPosition;
        public Vector3 LocalScale;
        public Quaternion LocalRotation;

        public GameObject(string name)
        {
            Name = name;
            Active = true;
            Components = new List<Component>();
            Children = new List<GameObject>();
            Parent = null;
            LocalRotation = new Quaternion(1, 0, 0, 0);
            LocalScale = new Vector3(1, 1, 1);
            EngineController.WorldObjects.Add(this);
        }
        public GameObject()
        {
            Name = string.Empty;
            Active = true;
            Components = new List<Component>();
            Children = new List<GameObject>();
            Parent = null;
            LocalRotation = new Quaternion(1, 0, 0, 0);
            LocalScale = new Vector3(1, 1, 1);
            EngineController.WorldObjects.Add(this);
        }

        #region Transform - Directions
        /// <summary>
        /// Forward Direction of this GameObject.
        /// </summary>
        public Vector3 Forward
        {
            get { return WorldRotation.RotateVectorZDirection(); }
        }
        /// <summary>
        /// Right Direction of this GameObject.
        /// </summary>
        public Vector3 Right
        {
            get { return Vector3.Cross_withYAxis(Forward).normalized; }
        }
        /// <summary>
        /// Up Direction of this GameObject.
        /// </summary>
        public Vector3 Up
        {
            get { return Vector3.Cross(Forward, Right).normalized; }
        }

        /// <summary>
        /// 3개의 방향(Forward, Right, Up)이 동시에 필요하다면 이 함수를 사용하세요. 성능상 이점이 있습니다.
        /// </summary>
        /// <returns></returns>
        public (Vector3, Vector3, Vector3) GetDirections()
        {
            var forward = Forward;
            var right = Vector3.Cross_withYAxis(forward).normalized;
            var up = Vector3.Cross(forward, right).normalized;
            return (forward, right, up);
        }
        #endregion

        #region Transform - WorldTransform
        /// <summary>
        /// Position in World Space of this GameObject.
        /// </summary>
        public Vector3 WorldPosition
        {
            get
            {
                if (Parent == null)
                    return LocalPosition;

                //재귀적으로 타고 올라가서 계산합니다.
                Vector3 worldParentPosition = Parent.WorldPosition;
                Quaternion worldParentRotation = Parent.WorldRotation;

                //Position은 Parent의 Rotation에 영향을 받습니다.
                return worldParentPosition + worldParentRotation.RotateVector(LocalPosition);
            }

            set
            {
                Vector3 point = value;
                if (Parent == null)
                {
                    LocalPosition = point;
                    return;
                }
                Quaternion inverseParentRotation = Parent.WorldRotation.Inverse();
                Vector3 diff = point - Parent.WorldPosition;

                LocalPosition = inverseParentRotation.RotateVector(diff);
            }
        }
        /// <summary>
        /// Rotation in World Space of this GameObject.
        /// </summary>
        public Quaternion WorldRotation
        {
            get
            {
                if (Parent == null)
                    return LocalRotation;
                return Parent.WorldRotation * LocalRotation;
            }
            set
            {
                Quaternion q = value;
                if (Parent == null)
                {
                    LocalRotation = q;
                    return;
                }

                LocalRotation = Parent.WorldRotation.Inverse() * q;
            }
        }
        /// <summary>
        /// Scale in World Space of this GameObject.
        /// </summary>
        public Vector3 WorldScale
        {
            get
            {
                if (Parent == null)
                    return LocalScale;
                return Vector3.ElementProduct(LocalScale, Parent.WorldScale);
            }

            set
            {
                Vector3 scale = value;
                if (Parent == null)
                {
                    LocalScale = scale;
                    return;
                }
                LocalScale = Vector3.ElementDivide(scale, Parent.WorldScale);
            }
        }
        public bool IsWorldActive
        {
            get
            {
                if (Active == false) return false;
                if (Parent == null) return Active;

                if (Parent.Active == false)
                    return false;
                if (Active == false)
                    return false;

                return true;
            }
        }
        #endregion

        #region Components
        public void AddComponent(Component component)
        {
            Components.Add(component);
            component.InitializeComponent(this);
        }
        public void RemoveComponent(Component component)
        {
            component.DeInitializeComponent(this);
            Components.Remove(component);
        }

        public bool TryGetComponent<T>(out T result) where T : Component
        {
            for (int i = 0; i < Components.Count; i++)
            {
                T v = Components[i] as T;
                if (v != null)
                {
                    result = v;
                    return true;
                }
            }
            result = null;
            return false;
        }
        #endregion

        public virtual void UpdateGameObject()
        {
            if (IsWorldActive == false)
                return;
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].UpdateComponent();
            }
        }

        public override string ToString()
        {
            return $"GameObject : {Name}";
        }
    }
}
