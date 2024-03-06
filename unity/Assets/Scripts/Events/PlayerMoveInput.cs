using UnityEngine;

namespace MyProject.Events
{
    [Event]
    public struct PlayerMoveInput
    {
        public Vector3 direction;

        public PlayerMoveInput(Vector3 direction)
        {
            this.direction = direction;
        }
    }
}
