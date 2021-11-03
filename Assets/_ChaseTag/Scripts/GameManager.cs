///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 02/10/2021 15:52
///-----------------------------------------------------------------

using Com.IsartDigital.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        [SerializeField] private List<GameObject> Maps;
        [SerializeField] private GameObject podiumMap;

        private int roundCounter = 1;

        private GameObject currentMap;

        public Timer GameTimer { get; private set; }
        public int RoundNumber { get => roundNumber; set => roundNumber = value; }
        public int RoundCounter { get => roundCounter; private set => roundCounter = value; }
        public int TimeLimit { get => timeLimit; set => timeLimit = value; }

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
            StartCoroutine(LoadMap());
            initGameSettings();
        }

        public void Restart()
        {
            wallStartBlock.SetActive(true);

            if (CollectibleManager.Instance != null) 
                CollectibleManager.Instance.DestroyAllCollectibles();

            RoundCounter++;

            Destroy(currentMap);

            StartCoroutine(LoadMap());
        }

        private IEnumerator LoadMap()
        {
            if (CollectibleManager.Instance != null) Destroy(CollectibleManager.Instance.gameObject);

            yield return null;

            var randomInt = UnityEngine.Random.Range(0, Maps.Count - 1);

            currentMap = Instantiate(Maps[randomInt]);

            Maps.RemoveAt(randomInt);
        }

        public void FinalMap()
        {
            wallStartBlock.SetActive(false);

            CollectibleManager.Instance.DestroyAllCollectibles();

            Destroy(currentMap);
            currentMap = Instantiate(podiumMap);

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
            GameTimer.Init(TimeLimit);
            GameTimer.OnTimerCompleted += GameTimer_OnTimerCompleted;
            wallStartBlock.SetActive(false);

            PlayerManager.Instance.GetComponent<PlayerInputManager>().DisableJoining();
        }

        private void GameTimer_OnTimerCompleted()
        {
            GameTimer.OnTimerCompleted -= GameTimer_OnTimerCompleted;
            GameTimer = null;

            //PlayerManager.Instance.SetPlayersControlScheme("Menu");

            Player winner;

            if (PlayerManager.Instance.TryGetPlayerWithMostElapsedTime(out winner))
            {
                OnWin?.Invoke(PlayerManager.Instance.GetPlayerId(winner), winner.MouseElapsedTime);
            }
            else
            {
                OnTie?.Invoke();
            }
            UIManager.Instance.gameStarted = false;
            //PlayerManager.Instance.DestroyPlayer();
        }

        private void initGameSettings()
        {

            //if use PlayerPref init here (don t forget first time opening)
            /* TimeLimit = playerpref time
               RoundNumber = playerpref round*/

            UIManager.Instance.initSliderValue(RoundNumber, TimeLimit);
        }
    }
}