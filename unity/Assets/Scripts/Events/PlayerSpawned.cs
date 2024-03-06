using UnityEngine;

namespace MyProject.Events
{
    [Event]
    public struct PlayerSpawned
    {
        public GameObject player;

        public PlayerSpawned(GameObject player)
        {
            this.player = player;
        }
    }
}
