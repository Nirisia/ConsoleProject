///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 02/10/2021 15:52
///-----------------------------------------------------------------

using Com.IsartDigital.Common;
using System;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
    public delegate void EndGameEventHandler(int playerId, float elapsedTime);
	public sealed class GameManager : MonoBehaviour {
	
		public static GameManager Instance { get; private set; }

        [SerializeField] private Timer timerPrefab = default;
        [SerializeField] private int timeLimit = 300;
        [SerializeField] private GameObject wallStartBlock = default;

        public Timer GameTimer { get; private set; }

        public event EndGameEventHandler OnWin;
        public event Action OnTie;

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

            if (GameTimer != null) GameTimer.OnTimerCompleted += GameTimer_OnTimerCompleted;

            OnWin = null;
            OnTie = null;
        }

        public void StartGame()
        {
            GameTimer = Instantiate(timerPrefab);
            GameTimer.Init(timeLimit);
            GameTimer.OnTimerCompleted += GameTimer_OnTimerCompleted;

            wallStartBlock.SetActive(false);
        }

        private void GameTimer_OnTimerCompleted()
        {
            GameTimer.OnTimerCompleted -= GameTimer_OnTimerCompleted;
            GameTimer = null;

            Player winner;

            if (PlayerManager.Instance.TryGetPlayerWithMostElapsedTime(out winner))
            {
                OnWin?.Invoke(PlayerManager.Instance.GetPlayerId(winner), winner.MouseElapsedTime);
            }
            else
            {
                OnTie?.Invoke();
            }

            PlayerManager.Instance.DestroyPlayer();
        }
    }
}