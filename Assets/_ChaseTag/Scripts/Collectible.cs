///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 29/09/2021 17:55
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
    public delegate void CollectibleEventHandler(Player player);
	public class Collectible : MonoBehaviour {

        public static event CollectibleEventHandler OnCollected;

        [SerializeField] private string playerTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                OnCollected?.Invoke(other.GetComponent<Player>());
                
                //Rajout de feedback
                Destroy(gameObject);
            }
        }
    }
}
