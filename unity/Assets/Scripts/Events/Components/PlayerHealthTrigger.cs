/// <auto-generated/>
using System;
using UnityEngine;
using UnityEngine.Events;

namespace MyProject.Events
{
    public class PlayerHealthTrigger : MonoBehaviour
    {
        public void Fire(System.Int32 health)
        {
            Channel.PlayerHealth.Dispatch(new MyProject.Events.PlayerHealth(health));
        }

        public void FireAt(System.Int32 health, UnityEngine.Vector3 __position)
        {
            Channel.PlayerHealth.Dispatch(new MyProject.Events.PlayerHealth(health), __position);
        }
    }
}