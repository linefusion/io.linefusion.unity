/// <auto-generated/>
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject.Events
{
    public class BulletHitListener : MonoBehaviour
    {
        public UnityEvent<BulletHit> onBulletHit = new();
        public void OnEnable()
        {
            Channel.BulletHit.Fired += OnTrigger;
        }

        public void OnDisable()
        {
            Channel.BulletHit.Fired -= OnTrigger;
        }

        private void OnTrigger(Event<MyProject.Events.BulletHit>.Args args)
        {
            onBulletHit.Invoke(args.Data);
        }
    }
}