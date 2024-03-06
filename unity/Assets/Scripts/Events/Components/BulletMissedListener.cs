/// <auto-generated/>
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject.Events
{
    public class BulletMissedListener : MonoBehaviour
    {
        public UnityEvent<BulletMissed> onBulletMissed = new();
        public void OnEnable()
        {
            Channel.BulletMissed.Fired += OnTrigger;
        }

        public void OnDisable()
        {
            Channel.BulletMissed.Fired -= OnTrigger;
        }

        private void OnTrigger(Event<MyProject.Events.BulletMissed>.Args args)
        {
            onBulletMissed.Invoke(args.Data);
        }
    }
}