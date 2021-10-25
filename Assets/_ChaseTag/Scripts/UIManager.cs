using Com.IsartDigital.ChaseTag.ChaseTag;
using Com.IsartDigital.ChaseTag.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.IsartDigital.ChaseTag
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private string txtAnimPlay = "FadeOut";
        [SerializeField] private string txtAnimGameOver = "GameOver";
        [SerializeField] private string txtAnimQuit = "Quit";
        [SerializeField] private string txtAnimReturnToTitlecard = "ReturnToTitlecard";
        [SerializeField] private string txtReady = "Ready";
        [SerializeField] private string txtNotReady = "Press A ...";
        [SerializeField] private Button btnPlay = default;
        [SerializeField] private Text txt_timer = default;

        [Serializable]
        public class PlayerUIInfo
        {
            [SerializeField] public Text txtPlayerReady = default;
            [SerializeField] public Text txt_GameOverPlayer = default;
            [SerializeField] public ParticleSystem fx_spawn = default;
            [SerializeField] public Text txtPlayerCollectible = default;
        }

        [SerializeField] private List<PlayerUIInfo> playerUIInfos;
        [SerializeField] private Vector3 offsetPlayerInMenu;


        [SerializeField] private AudioSource audioSourceMusic = default;
        [SerializeField] private AudioSource audioSourceFx = default;
        [SerializeField] private AudioClip win = default;
        [SerializeField] private AudioClip cheers = default;
        [SerializeField] private AudioClip boo = default;
        [SerializeField] private AudioClip lastSeconds = default;

        [SerializeField] private InputAction inputPause = default;
        [SerializeField] private InputAction inputQuit = default;

        private Animator animator;

        public bool gameOver = false;
        public bool gameStarted = false;
        public bool isQuitting = false;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            animator = GetComponent<Animator>();
            btnPlay.interactable = false;

            GameManager.Instance.OnWin += DisplayWin;
            GameManager.Instance.OnTie += DisplayTie;

            inputPause.performed += DisplayQuit;
            inputQuit.performed += Quit;
        }

        private void Update()
        {
            DisplayTimeUpdate();

            
        }

        public void StartTimer()
        {
            GameManager.Instance.StartGame();
            audioSourceMusic.Play();

            inputPause.Enable();
        }

        public void StartPlayerInStage()
        {
            for (int i = 0; i < PlayerManager.Instance.playerCount; i++)
            {
                PlayerManager.Instance.playerInfos[i].player.Resume();
            }
        }

        public void StartPlayerControls()
        {
            PlayerManager.Instance.SetPlayersControlScheme("Player");
            gameStarted = true;
        }

        private void DisplayTimeUpdate()
        {
            if (!gameStarted) return;

            if (GameManager.Instance.GameTimer != null)
            {
                int remainingTime = (int) GameManager.Instance.GameTimer.InvertedElapsedTime;
                txt_timer.text = remainingTime.ToString();

                if (remainingTime == 10)
                {
                    audioSourceFx.clip = lastSeconds;
                    audioSourceFx.Play();
                }
            }

            for (int i = 0; i < PlayerManager.Instance.playerCount; i++)
            {
                playerUIInfos[i].txtPlayerCollectible.text =
                    TimeDisplayTool.DisplayTime(PlayerManager.Instance.playerInfos[i].player.MouseElapsedTime, 1);
            }
        }

        public void ReplacePlayerInMenu(int id, Transform player)
        {
            if (id > playerUIInfos.Count) return;

            player.position = playerUIInfos[id].fx_spawn.transform.position + offsetPlayerInMenu;
            PlayerManager.Instance.playerInfos[id].player.Stop();
            playerUIInfos[id].fx_spawn.Play();
            playerUIInfos[id].txtPlayerReady.text = txtNotReady;
        }

        public void IsReady(int PlayerId, bool isReady)
        {
            if (PlayerId < 0 || PlayerId >= PlayerManager.Instance.playerCount) return;

            if (isReady)
                SetReady(PlayerId, isReady, txtReady);
            else
                SetReady(PlayerId, isReady, txtNotReady);

            if (PlayerManager.Instance.AllPlayersReady())
                btnPlay.interactable = true;
            else
                btnPlay.interactable = false;
        }

        private void SetReady(int PlayerId, bool isReady, string txtIsReady)
        {
            PlayerManager.Instance.playerInfos[PlayerId].isReady = isReady;
            playerUIInfos[PlayerId].txtPlayerReady.text = txtIsReady;
        }

        public void ResetReady()
        {
            for (int i = 0; i < 4; i++)
            {
                PlayerManager.Instance.playerInfos[i].isReady = false;
            }

            btnPlay.interactable = false;
        }

        public void BtnPlay()
        {
            if (PlayerManager.Instance.AllPlayersReady() && !gameStarted)
            {
                Debug.Log("Game Starting");
                animator.SetTrigger(txtAnimPlay);

                audioSourceFx.Play();
            }
        }

        public void DisplayTie()
        {
            foreach (var playerUIInfo in playerUIInfos)
            {
                playerUIInfo.txt_GameOverPlayer.text = "TIE";
            }

            animator.SetTrigger(txtAnimGameOver);

            audioSourceMusic.Stop();

            audioSourceFx.clip = boo;
            audioSourceFx.Play();
        }

        private void DisplayWin(int playerId, float elapsedTime)
        {
            Debug.Log(playerId + " : " + elapsedTime);

            for (int i = 0; i < PlayerManager.Instance.playerCount; i++)
            {
                if (playerId == i)
                    playerUIInfos[i].txt_GameOverPlayer.text = "WIN";
                else
                    playerUIInfos[i].txt_GameOverPlayer.text = "LOSe";
            }

            animator.SetTrigger(txtAnimGameOver);

            audioSourceMusic.Stop();
            audioSourceMusic.clip = win;
            audioSourceMusic.loop = false;
            audioSourceMusic.Play();

            audioSourceFx.clip = cheers;
            audioSourceFx.Play();
        }

        public void Pause()
        {
            Time.timeScale = 0;
            audioSourceMusic.Pause();
        }

        public void UnPause()
        {
            Time.timeScale = 1;
            audioSourceMusic.UnPause();
        }

        public void DisplayQuit(InputAction.CallbackContext callback)
        {
            if (!isQuitting)
                inputQuit.Enable();
            else
                inputQuit.Disable();

            isQuitting = !isQuitting;
            animator.SetTrigger(txtAnimQuit);
        }

        public void Quit(InputAction.CallbackContext callback)
        {
            UnPause();
            isQuitting = false;
            inputPause.Disable();
            inputQuit.Disable();
            SceneManager.LoadScene(0);
        }

        public void GameOverToTitleCard()
        {
            animator.SetTrigger(txtAnimReturnToTitlecard);
        }

        public void SetGameOver()
        {
            gameOver = true;
        }

        private void OnDestroy()
        {
            Instance = null;

        }
    }
}