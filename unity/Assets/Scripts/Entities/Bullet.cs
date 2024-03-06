using MyProject.Events;
using UnityEngine;

namespace MyProject.Entities
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 2.0f;

        [SerializeField]
        private float maxDistance = 15.0f;

        [SerializeField]
        private float raycastLength = 0.15f;
        
        [SerializeField]
        private Transform capsuleStart;

        [SerializeField]
        private Transform capsuleEnd;

        [SerializeField]
        private float capsuleRadius = 0.5f;

        private Vector3 spawnPosition;
        private RaycastHit raycastHit;

        private void Start()
        {
            spawnPosition = transform.position;
            Channel.BulletSpawned.FireAt(transform.rotation.eulerAngles.normalized, transform.position);
        }

        public void Update()
        {
            transform.position += transform.forward * moveSpeed * Time.deltaTime;

            if (Physics.CapsuleCast(capsuleStart.position, capsuleEnd.position, capsuleRadius, transform.forward, out raycastHit, raycastLength))
            {
                if (raycastHit.collider.gameObject.tag == "Player")
                {
                    Channel.PlayerHit.FireAt(transform.position);
                }
                else
                {
                    Channel.BulletMissed.Fire();
                }
                Destroy(gameObject);
            }

            if (Vector3.Distance(transform.position, spawnPosition) >= maxDistance)
            {
                Channel.BulletMissed.Fire();
                Destroy(gameObject);
            }
        }
    }
}
