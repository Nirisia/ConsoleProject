///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 30/09/2021 19:54
///-----------------------------------------------------------------

using Com.IsartDigital.ChaseTag.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public sealed class CollectibleManager : MonoBehaviour {
	
		public static CollectibleManager Instance { get; private set; }

        private List<Collectible> collectibles = new List<Collectible>();

        [SerializeField] private Collectible collectiblePrefab = default;
        
        [SerializeField] private float collectibleRespawnDelay = 0f;
        private WaitForSeconds respawnDelay;
        private Coroutine respawnCoroutine;

        [SerializeField] private int numSimultaneousCollectibles = 1;

        [Tooltip("make sure both spawnZoneBottomLeftAngle and spawnZoneTopRightAngle have the same Y coordinate")]
        [SerializeField] private Transform spawnZoneBottomLeftAngle = default;
        [Tooltip("make sure both spawnZoneBottomLeftAngle and spawnZoneTopRightAngle have the same Y coordinate")]
        [SerializeField] private Transform spawnZoneTopRightAngle = default;

        [SerializeField] private string groundTag = "Ground";

		private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("Trying to create multiple instances of singleton script, creation denied");
                Destroy(gameObject);
            }

            Collectible.OnCollected += Collectible_OnCollected;
            respawnDelay = new WaitForSeconds(collectibleRespawnDelay);
        }

        private void Collectible_OnCollected(Collectible collectible)
        {
            collectibles.Remove(collectible);

            //if (collectibles.Count < numSimultaneousCollectibles && respawnCoroutine == null)
            //{
            //    respawnCoroutine = StartCoroutine(SpawnNewCollectible());
            //}
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            Collectible.OnCollected -= Collectible_OnCollected;

            DestroyAllCollectibles();
        }

        public void DestroyAllCollectibles()
        {
            int length = collectibles.Count;

            for (int i = length - 1; i >= 0; i--)
            {
                Destroy(collectibles[i]);

                collectibles.RemoveAt(i);
            }
        }

        private IEnumerator SpawnNewCollectible()
        {
            yield return respawnDelay;

            Collectible collectible = Instantiate(collectiblePrefab);
            collectibles.Add(collectible);
            Vector3 position = Vector3.zero;

            RaycastHit hit;
            bool positionIsValid = false;

            while (!positionIsValid)
            {
                position = RandomTool.GetRandomPositionInCube(spawnZoneBottomLeftAngle.position, spawnZoneTopRightAngle.position);

                if (Physics.Raycast(position, Vector3.down, out hit))
                {
                    positionIsValid = hit.collider.CompareTag(groundTag);
                }
            }

            collectible.transform.position = position;

            respawnCoroutine = null;

            if (collectibles.Count < numSimultaneousCollectibles)
            {
                respawnCoroutine = StartCoroutine(SpawnNewCollectible());
            }
        }

        public void ResetCollectible()
        {
            if (respawnCoroutine == null)
            {
                respawnCoroutine = StartCoroutine(SpawnNewCollectible());
            }
        }
	}
}
