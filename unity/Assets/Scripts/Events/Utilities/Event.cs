using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using UnityEngine.UIElements;

namespace MyProject.Events
{
    public class Event<T>
        where T : struct
    {
        public class Args : EventArgs
        {
            public T Data { get; private set; }
            public UnityEngine.Vector3 Position { get; private set; } = UnityEngine.Vector3.zero;
            public bool IsPositional { get; private set; } = false;

            public Args(T data)
            {
                this.Data = data;
            }

            public Args(T data, UnityEngine.Vector3 position)
            {
                this.Data = data;
                this.Position = position;
                this.IsPositional = true;
            }
        }

        public delegate void Handler(Args data);

        protected Handler handlers;

        public event Handler Fired
        {
            add
            {
                handlers += value;
            }
            remove
            {
                handlers -= value;                   
            }
        }
        
        public void Dispatch(T data)
        {
            handlers?.Invoke(new Args(data));
        }

        public void Dispatch(T data, UnityEngine.Vector3 position)
        {
            handlers?.Invoke(new Args(data, position));
        }

        protected void Reset()
        {
            handlers = null;
        }
    }
}
