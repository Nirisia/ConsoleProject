using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class WallRipple : MonoBehaviour
    {
        public GameObject ripplesVFX;
        public Vector3 offset;
        public Quaternion rotation;

        private Material mat;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                var ripples = Instantiate(ripplesVFX, transform);

                ripples.transform.position = collision.contacts[0].point + offset;
                ripples.transform.rotation = rotation;
            }
        }
    }
}
