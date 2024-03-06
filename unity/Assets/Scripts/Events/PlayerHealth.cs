using UnityEngine;

namespace MyProject.Events
{
    [Event]
    public struct PlayerHealth
    {
        public int health;

        public PlayerHealth(int health)
        {
            this.health = health;
        }
    }
}
