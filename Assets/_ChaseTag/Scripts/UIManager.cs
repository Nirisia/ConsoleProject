using Com.IsartDigital.ChaseTag.ChaseTag;
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

        private Animator animator;
        private bool isPlayer1Ready = false;
        private bool isPlayer2Ready = false;
        
        public bool gameOver = false;
        public bool gameStarted = false;

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

        private void DisplayTimeUpdate()
        {
            if (GameManager.Instance.GameTimer != null)
            {
                int remainingTime = (int)GameManager.Instance.GameTimer.InvertedElapsedTime;
                txt_timer.text = remainingTime.ToString();
            }
        }

        public void ReplacePlayerInMenu(int id, Transform player)
        {
            if (id == 0)
            {
                player.position = player1Pos;
                PlayerManager.Instance.Player1 = player.GetComponent<Player>();
                PlayerManager.Instance.Player1.GetComponent<Player>().Stop();
            }
            else
            {
                player.position = player2Pos;
                PlayerManager.Instance.Player2 = player.GetComponent<Player>();
                PlayerManager.Instance.Player2.GetComponent<Player>().Stop();
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

            }
        }

        public void DisplayTie()
        {
            txt_GameOverPlayer1.text = "TIE";
            txt_GameOverPlayer2.text = "TIE";
            animator.SetTrigger(txtAnimGameOver);
        }

        public void DisplayWin(int playerId, PlayerState role)
        {
            if (playerId == 0)
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
