using Com.IsartDigital.ChaseTag.ChaseTag;
using Com.IsartDigital.ChaseTag.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Com.IsartDigital.ChaseTag
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private InputAction select;
        [SerializeField] private string txtAnimPlay = "FadeOut";
        [SerializeField] private string txtAnimGameOver = "GameOver";
        [SerializeField] private string txtAnimQuit = "Quit";
        [SerializeField] private string txtAnimReturnToTitlecard = "ReturnToTitlecard";
        [SerializeField] private string txtReady = "Ready";
        [SerializeField] private string txtNotReady = "Press A ...";
        [SerializeField] private Button btnPlay = default;
        [SerializeField] private Text txtPlayer1Ready = default;
        [SerializeField] private Text txtPlayer2Ready = default;
        [SerializeField] private Vector3 player1Pos = default;
        [SerializeField] private Vector3 player2Pos = default;
        [SerializeField] private Text txt_timer = default;
        [SerializeField] private Text txt_GameOverPlayer1 = default;
        [SerializeField] private Text txt_GameOverPlayer2 = default;
        [SerializeField] private ParticleSystem fx_spawnP1 = default;
        [SerializeField] private ParticleSystem fx_spawnP2 = default;
        [SerializeField] private Text txtPlayer1Collectible = default;
        [SerializeField] private Text txtPlayer2Collectible = default;

        [SerializeField] private AudioSource audioSourceMusic = default;
        [SerializeField] private AudioSource audioSourceFx = default;
        [SerializeField] private AudioClip win = default;
        [SerializeField] private AudioClip cheers = default;
        [SerializeField] private AudioClip boo = default;
        [SerializeField] private AudioClip lastSeconds = default;


        private Animator animator;
        private bool isPlayer1Ready = false;
        private bool isPlayer2Ready = false;
        
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
            txtPlayer1Ready.text = txtNotReady;
            txtPlayer2Ready.text = txtNotReady;

            select.performed += ctx => BtnPlay();
            select.Enable();

            GameManager.Instance.OnWin += DisplayWin;
            GameManager.Instance.OnTie += DisplayTie;
        }

        private void Update()
        {
            DisplayTimeUpdate();
        }

        public void StartTimer()
        {
            GameManager.Instance.StartGame();
            audioSourceMusic.Play();
        }

        private void DisplayTimeUpdate()
        {
            if (GameManager.Instance.GameTimer != null)
            {
                int remainingTime = (int)GameManager.Instance.GameTimer.InvertedElapsedTime;
                txt_timer.text = remainingTime.ToString();

                if (remainingTime == 10)
                {
                    audioSourceFx.clip = lastSeconds;
                    audioSourceFx.Play();
                }
            }

            if (PlayerManager.Instance.Player1 != null)
            {
                txtPlayer1Collectible.text = TimeDisplayTool.DisplayTime(PlayerManager.Instance.Player1.MouseElapsedTime, 1);
            }

            if (PlayerManager.Instance.Player2 != null)
            {
                txtPlayer2Collectible.text = TimeDisplayTool.DisplayTime(PlayerManager.Instance.Player2.MouseElapsedTime, 1);
            }
        }

        public void ReplacePlayerInMenu(int id, Transform player)
        {
            if (id == 0)
            {
                player.position = player1Pos;
                PlayerManager.Instance.Player1 = player.GetComponent<Player>();
                PlayerManager.Instance.Player1.GetComponent<Player>().Stop();
                fx_spawnP1.Play();
            }
            else
            {
                player.position = player2Pos;
                PlayerManager.Instance.Player2 = player.GetComponent<Player>();
                PlayerManager.Instance.Player2.GetComponent<Player>().Stop();
                fx_spawnP2.Play();
            }
        }

        public void IsReady(int PlayerId, bool isReady)
        {
            if (isReady)
                SetReady(PlayerId, isReady, txtReady);
            else
                SetReady(PlayerId, isReady, txtNotReady);

            if (isPlayer1Ready && isPlayer2Ready)
                btnPlay.interactable = true;
            else
                btnPlay.interactable = false;
        }

        private void SetReady(int PlayerId, bool isReady, string txtIsReady)
        {
            if (PlayerId == 0)
            {
                isPlayer1Ready = isReady;
                txtPlayer1Ready.text = txtIsReady;
            }
            else
            {
                isPlayer2Ready = isReady;
                txtPlayer2Ready.text = txtIsReady;
            }
        }

        public void ResetReady()
        {
            isPlayer1Ready = false;
            isPlayer2Ready = false;
            btnPlay.interactable = false;
        }

        public void BtnPlay()
        {
            if (isPlayer1Ready && isPlayer2Ready && !gameStarted)
            {
                animator.SetTrigger(txtAnimPlay);
                gameStarted = true;

                select.Disable();

                PlayerManager.Instance.Player1.GetComponent<Player>().Resume();
                PlayerManager.Instance.Player2.GetComponent<Player>().Resume();

                audioSourceFx.Play();
            }
        }

        public void DisplayTie()
        {
            txt_GameOverPlayer1.text = "TIE";
            txt_GameOverPlayer2.text = "TIE";
            animator.SetTrigger(txtAnimGameOver);

            audioSourceMusic.Stop();

            audioSourceFx.clip = boo;
            audioSourceFx.Play();
        }

        private void DisplayWin(int playerId, float elapsedTime)
        {
            Debug.Log(playerId + " : " + elapsedTime);

            if (playerId == 1)
            {
                txt_GameOverPlayer1.text = "WIN";
                txt_GameOverPlayer2.text = "LOSE";
            }
            else
            {
                txt_GameOverPlayer1.text = "LOSE";
                txt_GameOverPlayer2.text = "WIN";
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
        }

        public void UnPause()
        {
            Time.timeScale = 1;
        }

        public void DisplayQuit()
        {
            isQuitting = true;
            animator.SetTrigger(txtAnimQuit);
        }

        public void RemoveQuit()
        {
            isQuitting = false;
            animator.SetTrigger(txtAnimQuit);
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
