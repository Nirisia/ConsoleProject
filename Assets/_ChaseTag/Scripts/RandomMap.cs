using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class RandomMap : MonoBehaviour
    {
        [SerializeField] private Transform[] Walls = default;
        [SerializeField] private Transform[] Traps = default;

        [SerializeField] private Vector2 minPos = default;
        [SerializeField] private Vector2 maxPos = default;

        [SerializeField] private string groundTag = "Ground";

        private void Start()
        {
            PlaceRandomly(Walls);
            PlaceRandomly(Traps);
        }

        private void PlaceRandomly(Transform[] listObj)
        {
            for (int i = 0; i < listObj.Length; i++)
            {
                var randomPos = new Vector3(GetRandomFloat(minPos.x, maxPos.x), 0, GetRandomFloat(minPos.y, maxPos.y));

                RaycastHit hit;
                if (Physics.Raycast(randomPos, Vector3.down, out hit))
                {
                    while (!hit.collider.CompareTag(groundTag))
                    {
                        randomPos = new Vector3(GetRandomFloat(minPos.x, maxPos.x), 0, GetRandomFloat(minPos.y, maxPos.y));
                    }
                }

                listObj[i].position = randomPos;
            }
        }

        public float GetRandomFloat(float min, float max, float step = 1f)
        {
            int multipliedMin = (int)(min / step);
            int multipliedMax = (int)(max / step);

            return Random.Range(multipliedMin, multipliedMax) * step;
        }

    }
}
