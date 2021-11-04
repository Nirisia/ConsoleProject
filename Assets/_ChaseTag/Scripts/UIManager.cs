using Com.IsartDigital.ChaseTag.ChaseTag;
using Com.IsartDigital.ChaseTag.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Com.IsartDigital.ChaseTag
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private string txtAnimPlay = "FadeOut";
        [SerializeField] private string txtAnimGameOver = "GameOver";
        [SerializeField] private string txtAnimQuit = "Quit";
        [SerializeField] private string txtAnimSetting = "Setting";
        [SerializeField] private string txtAnimReturnToTitlecard = "ReturnToTitlecard";
        [SerializeField] private string txtFinalWin = "FinalWin";
        [SerializeField] private string txtReady = "Ready";
        [SerializeField] private string txtNotReady = "Press A ...";
        [SerializeField] private Button btnPlay = default;
        [SerializeField] private Text txt_timer = default;
        [SerializeField] public Text txt_GameOverPlayer = default;
        [SerializeField] public GameObject imgScore = default;
        [SerializeField] public Text DisplayRoundStart = default;
        [SerializeField] public Text DisplayRoundHUD = default;

        [SerializeField] public Text txtSetting_Round = default;
        [SerializeField] public Text txtSetting_Time = default;
        
        [SerializeField] public Slider sliderSetting_Round = default;
        [SerializeField] public Slider sliderSetting_Time = default;

        [Serializable]
        public class PlayerUIInfo
        {
            [SerializeField] public Text txtPlayerReady = default;
            [SerializeField] public ParticleSystem fx_spawn = default;
            [SerializeField] public Text txtPlayerCollectible = default;
            [SerializeField] public GameObject scoreParent = default;
        }

        [SerializeField] private List<PlayerUIInfo> playerUIInfos;
        [SerializeField] private Vector3 offsetPlayerInMenu;


        [SerializeField] private AudioSource audioSourceMusic = default;
        [SerializeField] private AudioSource audioSourceFx = default;
        [SerializeField] private AudioClip win = default;
        [SerializeField] private AudioClip music = default;
        [SerializeField] private AudioClip introMusic = default;
        [SerializeField] private AudioClip cheers = default;
        [SerializeField] private AudioClip boo = default;
        [SerializeField] private AudioClip lastSeconds = default;

        private Animator animator;

        public bool gameStarted = false;
        public bool isQuitting = false;
        public bool isSetting = false;
        public bool isHelp = false;
        public int helpPage = 2;
        public int helpPageCounter = 0;

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

            DisplayRoundHUD.text = (GameManager.Instance.RoundCounter) + " / " + GameManager.Instance.RoundNumber + "\nROUND";
        }

        private void Update()
        {
            DisplayTimeUpdate();

            
        }

        public void StartTimer()
        {
            GameManager.Instance.StartGame();
            audioSourceMusic.clip = music;
            audioSourceMusic.Play();
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
            StartPlayerInStage();
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

        public void DisplaySpawnEffect(int id)
        {
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
            if (PlayerManager.Instance.AllPlayersReady() && !gameStarted && !isSetting)
            {
                PlayerManager.Instance.SetPlayersControlScheme("No Input");
                Debug.Log("Game Starting");
                animator.SetTrigger(txtAnimPlay);

                audioSourceFx.Play();

                //CollectibleManager.Instance.ResetCollectible();

                HidePlayerNotPlaying();

                gameStarted = true;

                PlayerManager.Instance.GetComponent<PlayerInputManager>().DisableJoining();
                GameManager.Instance.SetFallingLD();

                Camera.main.GetComponent<CameraFollow>().IsSarting = true;
            }
        }

        public void DisplayTie()
        {
            txt_GameOverPlayer.text = "NOBODY";

            animator.SetTrigger(txtAnimGameOver);

            audioSourceMusic.Stop();

            audioSourceFx.clip = boo;
            audioSourceFx.Play();

            Time.timeScale = 0;
        }

        private void DisplayWin(int playerId, float elapsedTime)
        {
            PlayerManager.Instance.playerInfos[playerId].score++;
            GameObject imgPoint = Instantiate(imgScore, playerUIInfos[playerId].scoreParent.transform);

            imgPoint.GetComponent<RawImage>().color = PlayerManager.Instance.playerInfos[playerId].color;

            PlayerManager.Instance.ResetAllPlayer();

            txt_GameOverPlayer.text = "PLAYER   " + (playerId+1);

            animator.SetTrigger(txtAnimGameOver);

            audioSourceMusic.Stop();
            audioSourceMusic.clip = win;
            audioSourceMusic.loop = false;
            audioSourceMusic.Play();

            audioSourceFx.clip = cheers;
            audioSourceFx.Play();

            Time.timeScale = 0;
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
            if (!gameStarted) return;
            
            isQuitting = !isQuitting;
            animator.SetTrigger(txtAnimQuit);
        }

        public void Quit(InputAction.CallbackContext callback)
        {
            if (!isQuitting) return;
            
            UnPause();
            isQuitting = false;
            SceneManager.LoadScene(0);
        }

        public void NextRound(InputAction.CallbackContext callback)
        {
            if (isQuitting || isSetting || gameStarted) return;

            gameStarted = true;

            Debug.Log("Round " + GameManager.Instance.RoundCounter + " sur " + GameManager.Instance.RoundNumber);

            DisplayRoundStart.text = "ROUND " + (GameManager.Instance.RoundCounter+1);
            DisplayRoundHUD.text = (GameManager.Instance.RoundCounter+1) + " / " + GameManager.Instance.RoundNumber + "\nROUND";

            if (GameManager.Instance.RoundCounter == GameManager.Instance.RoundNumber)
            {
                DisplayRoundHUD.text = GameManager.Instance.RoundCounter + " / " + GameManager.Instance.RoundNumber + "\nROUND";
                DisplayFinalWin();
                return;
            }

            animator.SetTrigger(txtAnimReturnToTitlecard);

            PlayerManager.Instance.ReplaceAllPlayer();

            audioSourceFx.clip = introMusic;
            audioSourceFx.Play();
            Time.timeScale = 1;

            GameManager.Instance.Restart();

        }

        private void DisplayFinalWin()
        {
            Time.timeScale = 1;
            animator.SetTrigger(txtFinalWin);
            GameManager.Instance.FinalMap();

            PlayerManager.Instance.setOnPodium();
        }

        public void GameOverToTitleCard()
        {
            animator.SetTrigger(txtAnimReturnToTitlecard);
        }

        public void DisplayHelp(InputAction.CallbackContext callback)
        {
            animator.SetTrigger("Help");

            if (helpPageCounter == 0 || helpPageCounter == helpPage)
            {
                EventSystem.current?.SetSelectedGameObject(null);

                if (!isHelp)
                {
                    PlayerManager.Instance.HidePlayer();
                    PlayerManager.Instance.GetComponent<PlayerInputManager>().DisableJoining();
                    EventSystem.current?.SetSelectedGameObject(sliderSetting_Round.gameObject);
                    isHelp = true;
                }
                else
                {
                    PlayerManager.Instance.ShowPlayer();
                    PlayerManager.Instance.GetComponent<PlayerInputManager>().EnableJoining();
                    isHelp = false;
                }
            }

            helpPageCounter++;

            if (helpPageCounter > helpPage)
                helpPageCounter = 0;
        }

        private void OnDestroy()
        {
            Instance = null;

        }

        public void setRoundNumberSlider(float value)
        {
            txtSetting_Round.text = value.ToString();
            GameManager.Instance.RoundNumber = (int)value;

            DisplayRoundHUD.text = (GameManager.Instance.RoundCounter) + " / " + GameManager.Instance.RoundNumber + "\nROUND";

            //set playerpref round here
        }

        public void setRoundTimeSlider(float value)
        {
            txtSetting_Time.text = value.ToString();
            GameManager.Instance.TimeLimit = (int)value;

            //set playerpref time here
        }

        public void initSliderValue(int round, int time)
        {
            sliderSetting_Round.value = round;
            sliderSetting_Time.value = time;

            setRoundNumberSlider(round);
            setRoundTimeSlider(time);
        }

        public void DisplaySetting(InputAction.CallbackContext callback)
        {
            animator.SetTrigger(txtAnimSetting);
            isSetting = !isSetting;

            if (isSetting)
            {
                PlayerManager.Instance.HidePlayer();
                PlayerManager.Instance.GetComponent<PlayerInputManager>().DisableJoining();
                EventSystem.current?.SetSelectedGameObject(sliderSetting_Round.gameObject);
            }
            else
            {
                PlayerManager.Instance.ShowPlayer();
                PlayerManager.Instance.GetComponent<PlayerInputManager>().EnableJoining();

                EventSystem.current?.SetSelectedGameObject(null);
            }
        }

        private void HidePlayerNotPlaying()
        {
            var playerNotPlaying = PlayerManager.Instance.playerCount;

            for (int i = playerUIInfos.Count - 1; i >= playerNotPlaying; i--)
            {
                playerUIInfos[i].scoreParent.SetActive(false);
            }

        }
    }
}