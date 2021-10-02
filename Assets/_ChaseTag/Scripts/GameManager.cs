///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 02/10/2021 15:52
///-----------------------------------------------------------------

using Com.IsartDigital.Common;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public sealed class GameManager : MonoBehaviour {
	
		public static GameManager Instance { get; private set; }

        [SerializeField] private Timer timerPrefab = default;
        public Timer GameTimer { get; private set; }

		private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("Trying to create multiple instances of singleton script, creation denied");
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            GameTimer = Instantiate(timerPrefab);
            GameTimer.Init(300);
            GameTimer.OnTimerCompleted += GameTimer_OnTimerCompleted;

            Player.OnMouseCaught += Player_OnMouseCaught;
        }

        private void Player_OnMouseCaught(Player player)
        {
            Player.OnMouseCaught -= Player_OnMouseCaught;

            //player wins
        }

        private void GameTimer_OnTimerCompleted()
        {
            GameTimer.OnTimerCompleted -= GameTimer_OnTimerCompleted;

            Player winner;

            if (PlayerManager.Instance.TryGetMousePlayer(out winner))
            {
                //winner wins
            }
            else
            {
                //tie
            }
        }
    }
}
