///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 29/09/2021 18:05
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public sealed class PlayerManager : MonoBehaviour {
	
		public static PlayerManager Instance { get; private set; }

        [SerializeField] private Player player1 = default;
        [SerializeField] private Player player2 = default;
		
		private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("Trying to create multiple instances of singleton script, creation denied");
                Destroy(gameObject);
            }

            Player.OnCollectibleCollected += Player_OnCollectibleCollected;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            Player.OnCollectibleCollected -= Player_OnCollectibleCollected;
        }
 
        private void Player_OnCollectibleCollected(Player player)
        {
            if (player1.NumCollectiblesCollected > player2.NumCollectiblesCollected)
            {
                player1.SetModeCat();
                player2.SetModeMouse();
            }
            else if (player1.NumCollectiblesCollected < player2.NumCollectiblesCollected)
            {
                player1.SetModeMouse();
                player2.SetModeCat();
            }
        }

        public bool TryGetMousePlayer(out Player player)
        {
            if (player1.CurrentState == PlayerState.MOUSE)
            {
                player = player1;
                return true;
            }
            else if (player2.CurrentState == PlayerState.CAT)
            {
                player = player2;
                return true;
            }
            else
            {
                player = null;
                return false;
            }
        }
	}
}
