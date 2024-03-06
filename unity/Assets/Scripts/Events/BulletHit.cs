using UnityEngine;
using UnityEngine.Events;

namespace MyProject.Events
{
    [Event]
    public struct BulletHit
    {
        public Vector3 position;

        public BulletHit(Vector3 position)
        {
            this.position = position;
        }
    }
}
