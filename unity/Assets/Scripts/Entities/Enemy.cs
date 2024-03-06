using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MyProject.Entities
{
    public class Enemy : MonoBehaviour
    {
        public float shotIntervalMin = 1.0f;
        public float shotIntervalMax = 4.0f;

        public Transform bulletSpawner;
        public GameObject bulletPrefab;

        private float nextShot = 0.0f;

        private void OnEnable()
        {
            nextShot = Time.time + Random.Range(shotIntervalMin, shotIntervalMax);
        }

        public void Update()
        {
            if (nextShot > Time.time)
            {
                return;
            }

            nextShot = Time.time + Random.Range(shotIntervalMin, shotIntervalMax);
            Instantiate(bulletPrefab, bulletSpawner.position, bulletSpawner.rotation).SetActive(true);
        }
    }
}
