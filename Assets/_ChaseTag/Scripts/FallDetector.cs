using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class FallDetector : MonoBehaviour
    {
        [SerializeField] private Vector3 respawnPosition;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.position = respawnPosition;
            }
        }
    }
}
