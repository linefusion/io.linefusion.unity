using UnityEngine;

namespace MyProject.Events
{
    [Event]
    public struct BulletSpawned
    {
        public Vector3 direction;
        
        public BulletSpawned(Vector3 direction)
        {
            this.direction = direction;
        }
    }
}
