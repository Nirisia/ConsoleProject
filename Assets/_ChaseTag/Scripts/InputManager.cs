using Com.IsartDigital.ChaseTag.ChaseTag;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

using nn.hid;

namespace Com.IsartDigital.ChaseTag
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputmanager = default;
        [SerializeField] private GameObject playerPrefab = default;

        private class PlayerState
        {
            public bool playerSpawned = false;
        }


        const int numberOfControllers = 4; //4 players
        PlayerState[] playerStates = new PlayerState[4];

#if true//UNITY_SWITCH && !UNITY_EDITOR
        NpadState[] npadStates = new NpadState[numberOfControllers];
#endif

        private void Start()
        {
            playerInputmanager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

            // 1. Preprocessor Directive
#if UNITY_SWITCH && !UNITY_EDITOR
            Npad.SetSupportedStyleSet(NpadStyle.JoyLeft | NpadStyle.JoyRight);
            NpadJoy.SetHoldType(NpadJoyHoldType.Horizontal);
            NpadId[] npadIds = { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };
            Npad.SetSupportedIdType(npadIds);


            // 2. Set the Arguments For the Applet
            ControllerSupportArg controllerSupportArgs = new ControllerSupportArg();
            controllerSupportArgs.SetDefault();
            controllerSupportArgs.playerCountMax = 4; // This will show 4 controller setup boxes in the in the applet 
            controllerSupportArgs.playerCountMin = 1; // Applet requires at least 1 player to connect

            // 3. (Optional) Suspend Unity Processes to Call the Applet 
            UnityEngine.Switch.Applet.Begin(); //call before calling a system applet to stop all Unity threads (including audio and networking)

            // 4. Call the Applet
            nn.hid.ControllerSupport.Show(controllerSupportArgs);

            // 5. (Optional) Resume the Suspended Unity Processes
            UnityEngine.Switch.Applet.End(); //always call this if you called Switch.Applet.Begin()

            for (int i = 0; i < numberOfControllers; i++){
                npadStates[i] = new NpadState();
            }

            // End the Preprocessor Directive
#endif
        }

        private void Update()
        {
            handleControllerInput(0, NpadId.No1);
            handleControllerInput(1, NpadId.No2);
        }

        void handleControllerInput(int controllerNumber, NpadId npadId)
        {
#if true// UNITY_SWITCH && !UNITY_EDITOR

            NpadStyle npadStyle = Npad.GetStyleSet(npadId);

            Npad.GetState(ref npadStates[controllerNumber], npadId, npadStyle);

            //if there is no Joy-Con connected, return.
            if (npadStyle == NpadStyle.None)
            {
                return;
            }

            bool JoinKey = npadStates[controllerNumber].GetButtonDown(
                    (npadStyle == NpadStyle.JoyRight) ? NpadButton.X : NpadButton.Left
                    );

            bool LeaveKey = npadStates[controllerNumber].GetButtonDown(
                    (npadStyle == NpadStyle.JoyRight) ? NpadButton.Plus : NpadButton.Minus
                    );

            if (JoinKey && UIManager.Instance.gameOver)
            {
                SceneManager.LoadScene(0);
            }
            else if (LeaveKey && !UIManager.Instance.isQuitting)
            {
                UIManager.Instance.DisplayQuit();
            }
            else if (LeaveKey && UIManager.Instance.isQuitting)
            {
                UIManager.Instance.RemoveQuit();
                UIManager.Instance.UnPause();
            }
            else if (JoinKey && UIManager.Instance.isQuitting)
            {
                SceneManager.LoadScene(0);
            }
            else if (JoinKey && (PlayerManager.Instance.Player1 == null || PlayerManager.Instance.Player2 == null))
            {
                var player = PlayerInput.Instantiate(playerPrefab,
                    controlScheme: (npadStyle == NpadStyle.JoyRight) ? "JoyconRight" : "JoyconLeft") ;
                

                if (npadStyle == NpadStyle.JoyRight)
                {
                    PlayerManager.Instance.Player1 = player.GetComponent<Player>();
                    UIManager.Instance.IsReady(0, true);
                    UIManager.Instance.ReplacePlayerInMenu(0, player.transform);
                }
                else if (npadStyle == NpadStyle.JoyLeft)
                {
                    PlayerManager.Instance.Player2 = player.GetComponent<Player>();
                    UIManager.Instance.IsReady(1, true);
                    UIManager.Instance.ReplacePlayerInMenu(1, player.transform);
                }

            } else if (JoinKey)
            {
                UIManager.Instance.BtnPlay();
            }
#endif
        }

        public void ReadyInput()
        {
            if (InputSystem.devices.Count == 0)
            {
                UIManager.Instance.IsReady(0, false);
                UIManager.Instance.IsReady(1, false);
            }
            else if (InputSystem.devices.Count == 1)
            {
                UIManager.Instance.IsReady(0, true);
                UIManager.Instance.IsReady(1, false);
            }
            else if (InputSystem.devices.Count == 2)
            {
                UIManager.Instance.IsReady(0, true);
                UIManager.Instance.IsReady(1, true);
            }
        }

        //public void OnDestroy()
        //{
        //    InputSystem.onDeviceChange -= (device, change) => ReadyInput();
        //    playerInputmanager.onPlayerJoined += ctx => ReadyInput();
        //    playerInputmanager.onPlayerLeft += ctx => ReadyInput();
        //}

    }
}
