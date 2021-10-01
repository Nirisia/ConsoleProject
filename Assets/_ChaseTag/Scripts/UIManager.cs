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
        [SerializeField] private string txtReady = "Ready";
        [SerializeField] private string txtNotReady = "Press A ...";
        [SerializeField] private Button btnPlay = default;
        [SerializeField] private Text txtPlayer1Ready = default;
        [SerializeField] private Text txtPlayer2Ready = default;
        [SerializeField] private Vector3 player1Pos = default;
        [SerializeField] private Vector3 player2Pos = default;

        private Animator animator;
        private bool isPlayer1Ready = false;
        private bool isPlayer2Ready = false;
        private Transform obj_player1;
        private Transform obj_player2;
        
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
        }

        public void ReplacePlayerInMenu(int id, Transform player)
        {
            if (id == 0)
                player.position = player1Pos;
            else
                player.position = player2Pos;
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

                var players = GameObject.FindGameObjectsWithTag("Player");
                players[0].GetComponent<Player>().StartMoving();
                players[1].GetComponent<Player>().StartMoving();

            }
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}
