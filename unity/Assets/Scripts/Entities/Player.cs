using MyProject.Events;

using System;

using UnityEngine;
using UnityEngine.EventSystems;

namespace MyProject.Entities
{
    public class Player : MonoBehaviour
    {
        public float speed = 5.0f;

        private void OnEnable()
        {
            Channel.PlayerMoveInput.Fired += OnPlayerInput;
            Channel.PlayerSpawned.Fire(this.gameObject);
        }

        private void OnDisable()
        {
            Channel.PlayerMoveInput.Fired -= OnPlayerInput;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void OnPlayerInput(Event<PlayerMoveInput>.Args args)
        {
            transform.position += speed * Time.deltaTime * args.Data.direction;
        }
    }
}

