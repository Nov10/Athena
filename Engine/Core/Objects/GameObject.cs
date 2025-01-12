using System;
using System.Collections.Generic;
using System.Linq;
using Athena.Maths;
using System.Text;
using System.Threading.Tasks;
using Athena;

namespace Athena.Engine.Core
{
    public class GameObject
    {
        public string Name;
        public bool Active;
        public List<Component> Components;

        public GameObject Parent;
        public List<GameObject> Children;

        public Vector3 LocalPosition;
        public Vector3 LocalScale;
        public Quaternion LocalRotation;

        public Vector3 Forward
        {
            get { return WorldRotation.RotateVectorZDirection(); }
        }
        public Vector3 Right
        {
            get { return Vector3.Cross_withYAxis(Forward).normalized; }
        }
        public Vector3 Up
        {
            get { return Vector3.Cross(Forward, Right).normalized; }
        }

        public Vector3 WorldPosition
        {
            get
            {
                if (Parent == null)
                    return LocalPosition;

                Vector3 worldParentPosition = Parent.WorldPosition;
                Quaternion worldParentRotation = Parent.WorldRotation;

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

                Vector3 diff = point - Parent.WorldPosition;

                LocalPosition = diff;
            }
        }

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

                LocalRotation = Parent.WorldRotation.Conjugate() * q;
            }
        }
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


        public void AddComponent(Component component)
        {
            Components.Add(component);
            component.Initialize(this);
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
        public virtual void Update()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].UpdateComponent();
            }
        }
        public GameObject(string name)
        {
            Name = name;
            Active = true;
            Components = new List<Component>();
            Children = new List<GameObject>();
            Parent = null;
            LocalRotation = new Quaternion(1, 0, 0, 0);
            LocalScale = new Vector3(1, 1, 1);
            MainWindow.WorldObjects.Add(this);
        }
        public GameObject()
        {
            Active = true;
            Components = new List<Component>();
            Children = new List<GameObject>();
            Parent = null;
            LocalRotation = new Quaternion(1, 0, 0, 0);
            LocalScale = new Vector3(1, 1, 1);
            MainWindow.WorldObjects.Add(this);
        }
    }
}
