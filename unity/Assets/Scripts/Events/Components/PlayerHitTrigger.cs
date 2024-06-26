/// <auto-generated/>
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject.Events
{
    public class PlayerHitTrigger : MonoBehaviour
    {
        public void Fire()
        {
            Channel.PlayerHit.Dispatch(new MyProject.Events.PlayerHit());
        }

        public void FireAt(UnityEngine.Vector3 position)
        {
            Channel.PlayerHit.Dispatch(new MyProject.Events.PlayerHit(), position);
        }
    }
}