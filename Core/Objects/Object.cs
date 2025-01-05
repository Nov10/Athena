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
        public Quaternion LocalRotation;

        public Vector3 Forward
        {
            get { return WorldRotation.RotateVector(new Vector3(0, 0, 1)); }
        }
        public Vector3 Right
        {
            get { return Vector3.Cross(new Vector3(0, 1, 0), Forward).normalized; }
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
                if(Parent == null)
                {
                    LocalRotation = q;
                    return;
                }

                LocalRotation = Parent.WorldRotation.Conjugate() * q;
            }
        }

        public List<Component> Components;

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
                if(v != null)
                {
                    result = v;
                    return true;
                }
            }
            result = null;
            return false;
        }

        public virtual void Update()
        {
            for(int i = 0; i < Components.Count; i++)
            {
                Components[i].UpdateComponent();
            }
        }

        public Object()
        {
            Components = new List<Component>();
            Children = new List<Object>();
            Parent = null;
            LocalRotation = new Quaternion(1, 0, 0, 0);
        }
    }
}
