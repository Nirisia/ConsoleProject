///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 29/09/2021 18:05
///-----------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public sealed class PlayerManager : MonoBehaviour {
	
		public static PlayerManager Instance { get; private set; }

        [SerializeField] private Player player1 = default;
        [SerializeField] private Player player2 = default;
        [SerializeField] private Color colorPlayer1 = default;
        [SerializeField] private Color colorPlayer2 = default;
        [SerializeField] private Text txtStatePlayer1 = default;
        [SerializeField] private Text txtStatePlayer2 = default;

        [Header("State name")]
        [SerializeField] private string txtStateNormal = "GET ITEM FIRST";
        [SerializeField] private string txtStateCat = "CAT";
        [SerializeField] private string txtStateMouse = "MOUSE";
        //[SerializeField] private Text txtCollectiblePlayer1 = default;
        //[SerializeField] private Text txtCollectiblePlayer2 = default;

        public Player Player1 {
            get { return player1; }
            set { 
                player1 = value;
                player1.GetComponentInChildren<Renderer>().material.color = colorPlayer1;
            }
        }
        public Player Player2
        {
            get { return player2; }
            set { 
                player2 = value;
                player2.GetComponentInChildren<Renderer>().material.color = colorPlayer2;
            }
        }

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

        public void ReturnPlayerToNormal()
        {
            player1.haveCrown = false;
            player2.haveCrown = false;

            player1.SetModeNormal();
            player2.SetModeNormal();

            Player_OnCollectibleCollected();
        }
 
        private void Player_OnCollectibleCollected(Player player = default)
        {
            if (/*player1.NumCollectiblesCollected < player2.NumCollectiblesCollected*/ player2.haveCrown)
            {
                player1.SetModeCat();
                player2.SetModeMouse();

                player1.SetSize(PlayerState.CAT);
                player2.SetSize(PlayerState.MOUSE);

                txtStatePlayer1.text = txtStateCat;
                txtStatePlayer2.text = txtStateMouse;
            }
            else if (/*player1.NumCollectiblesCollected > player2.NumCollectiblesCollected*/ player1.haveCrown)
            {
                player1.SetModeMouse();
                player2.SetModeCat();

                player1.SetSize(PlayerState.MOUSE);
                player2.SetSize(PlayerState.CAT);

                txtStatePlayer1.text = txtStateMouse;
                txtStatePlayer2.text = txtStateCat;
            }
            else if (!player1.haveCrown && !player2.haveCrown)
            {
                player1.SetSize(PlayerState.NORMAL);
                player2.SetSize(PlayerState.NORMAL);

                txtStatePlayer1.text = txtStateNormal;
                txtStatePlayer2.text = txtStateNormal;
            }

            //txtCollectiblePlayer1.text = player1.NumCollectiblesCollected.ToString();
            //txtCollectiblePlayer2.text = player2.NumCollectiblesCollected.ToString();
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

        public void DestroyPlayer()
        {
            Destroy(player1.gameObject);
            Destroy(player2.gameObject);
        }

        public int GetPlayerId(Player player)
        {
            if (player == player1) return 1;
            else return 2;
        }

        public bool TryGetPlayerWithMostElapsedTime(out Player player)
        {
            if (player1.MouseElapsedTime == player2.MouseElapsedTime)
            {
                player = null;
                return false;
            }

            player = player1.MouseElapsedTime > player2.MouseElapsedTime ? player1 : player2;
            return true;
        }

        public float GetPlayerMouseElapsedTime(int playerId)
        {
            if (playerId == 1) return player1.MouseElapsedTime;
            if (playerId == 2) return player2.MouseElapsedTime;

            return 0;
        }
    }
}
