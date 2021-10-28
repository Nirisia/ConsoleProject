///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 02/10/2021 15:52
///-----------------------------------------------------------------

using Com.IsartDigital.Common;
using System;
using System.Collections;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
    public delegate void EndGameEventHandler(int playerId, float elapsedTime);
	public sealed class GameManager : MonoBehaviour {
	
		public static GameManager Instance { get; private set; }

        [SerializeField] private Timer timerPrefab = default;
        [SerializeField] private GameObject wallStartBlock = default;

        [Header("Game Settings")]
        [SerializeField] private int timeLimit = 300;
        [SerializeField] private int roundNumber = 6;
        [SerializeField] private float timeBeforeMovePodium = 3f;

        [SerializeField] private GameObject[] Maps;

        private int roundCounter = 1;

        private GameObject currentMap;

        public Timer GameTimer { get; private set; }
        public int RoundNumber { get => roundNumber; private set => roundNumber = value; }
        public int RoundCounter { get => roundCounter; private set => roundCounter = value; }

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

        private void Start()
        {
            currentMap = Instantiate(Maps[0]);
        }

        public void Restart()
        {
            wallStartBlock.SetActive(true);

            CollectibleManager.Instance.DestroyAllCollectibles();
            CollectibleManager.Instance.ResetCollectible();

            RoundCounter++;

            Destroy(currentMap);
            currentMap = Instantiate(Maps[roundCounter-1]);
        }

        public void FinalMap()
        {
            wallStartBlock.SetActive(false);

            CollectibleManager.Instance.DestroyAllCollectibles();

            Destroy(currentMap);
            currentMap = Instantiate(Maps[roundCounter]);

            StartCoroutine(WinWaitBeforeMoving());

            CameraFollow.Instance.isWinPos = true;
            CameraFollow.Instance.WinPosition();
        }

        private IEnumerator WinWaitBeforeMoving()
        {
            yield return new WaitForSeconds(timeBeforeMovePodium);

            PlayerManager.Instance.SetPlayersControlScheme("Player");
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

            PlayerManager.Instance.SetPlayersControlScheme("Menu");

            Player winner;

            if (PlayerManager.Instance.TryGetPlayerWithMostElapsedTime(out winner))
            {
                OnWin?.Invoke(PlayerManager.Instance.GetPlayerId(winner), winner.MouseElapsedTime);
            }
            else
            {
                OnTie?.Invoke();
            }

            //PlayerManager.Instance.DestroyPlayer();
        }
    }
}