using Com.IsartDigital.ChaseTag.ChaseTag;
using nn.hid;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Switch;
using UnityEngine.SceneManagement;

namespace Com.IsartDigital.ChaseTag
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputmanager = default;
        [SerializeField] private GameObject playerPrefab = default;

        [Header("Testing")]
        [SerializeField] private bool withJoyCon = default;

        private bool player1Spawned = false;
        private bool player2Spawned = false;

        const int numberOfControllers = 3; //4 players + handheld
        public NpadState[] npadStates = new NpadState[numberOfControllers];

        private void Start()
        {
            if (withJoyCon)
            {
                // init the controller states
                for (int i = 0; i < numberOfControllers; i++)
                {
                    npadStates[i] = new NpadState();
                }

                //controller initialization
                Npad.Initialize();
                Npad.SetSupportedStyleSet(NpadStyle.JoyLeft | NpadStyle.JoyRight);

                NpadJoy.SetHoldType(NpadJoyHoldType.Horizontal);
                NpadJoy.SetHandheldActivationMode(NpadHandheldActivationMode.Dual);
                NpadId[] npadIds = { NpadId.Handheld, NpadId.No1, NpadId.No2 };
                Npad.SetSupportedIdType(npadIds);
            }

            playerInputmanager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

            // NOT WORKING
            /*InputSystem.onDeviceChange += (device, change) => ReadyInput();
            playerInputmanager.onPlayerJoined += ctx => ReadyInput();
            playerInputmanager.onPlayerLeft += ctx => ReadyInput();*/

        }

        void handleControllerInput(int controllerNumber, NpadId npadId)
        {
            NpadStyle npadStyle = Npad.GetStyleSet(npadId);
            Npad.GetState(ref npadStates[controllerNumber], npadId, npadStyle);

            //if there is no Joy-Con connected, return.
            if (npadStyle == NpadStyle.None)
            {   
                return;
            }

            if (npadStates[controllerNumber].GetButtonDown(NpadButton.A) && UIManager.Instance.gameOver)
            {
                SceneManager.LoadScene(0);
            }
            else if (npadStates[controllerNumber].GetButtonDown(NpadButton.Plus) && !UIManager.Instance.isQuitting)
            {
                UIManager.Instance.DisplayQuit();
            }
            else if (npadStates[controllerNumber].GetButtonDown(NpadButton.Plus) && UIManager.Instance.isQuitting)
            {
                UIManager.Instance.RemoveQuit();
                UIManager.Instance.UnPause();
            }
            else if (npadStates[controllerNumber].GetButtonDown(NpadButton.A) && UIManager.Instance.isQuitting)
            {
                Application.Quit(0);
                Debug.Log("You leaved the game !");
            }
            else if (npadStates[controllerNumber].GetButtonDown(NpadButton.A) && !player2Spawned)
            {
                Debug.Log("Spawn Player");
                var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "Player2", pairWithDevice: NPad.current);
                player2Spawned = true;
                UIManager.Instance.IsReady(1, true);
                UIManager.Instance.ReplacePlayerInMenu(1, player.transform);
            }
            else if (npadStates[controllerNumber].GetButtonDown(NpadButton.Left) && !player1Spawned)
            {
                Debug.Log("Spawn Player");
                var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "Player", pairWithDevice: NPad.current);
                player1Spawned = true;
                UIManager.Instance.IsReady(0, true);
                UIManager.Instance.ReplacePlayerInMenu(0, player.transform);
            }
            else if (npadStates[controllerNumber].GetButtonDown(NpadButton.Left) || npadStates[controllerNumber].GetButtonDown(NpadButton.A))
            {
                UIManager.Instance.BtnPlay();
            }

            if (npadStyle == NpadStyle.JoyRight && player2Spawned && player1Spawned)
            {
                PlayerManager.Instance.Player1.movementInput.x = npadStates[controllerNumber].analogStickR.y;
                PlayerManager.Instance.Player1.movementInput.y = -npadStates[controllerNumber].analogStickR.x;
                if(npadStates[controllerNumber].GetButtonDown(NpadButton.A))
                    PlayerManager.Instance.Player1.Dash();
            }
            else if (npadStyle == NpadStyle.JoyLeft && player2Spawned && player1Spawned)
            {
                PlayerManager.Instance.Player2.movementInput.x = -npadStates[controllerNumber].analogStickL.y;
                PlayerManager.Instance.Player2.movementInput.y = npadStates[controllerNumber].analogStickL.x;
                if (npadStates[controllerNumber].GetButtonDown(NpadButton.Left))
                    PlayerManager.Instance.Player2.Dash();
            }
        }

        private void Update()
        {
            if (!withJoyCon)
                HandleKeyboard();
            else
            {
                handleControllerInput(0, NpadId.No1);
                handleControllerInput(1, NpadId.No2);
            }
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

        private void HandleKeyboard()
        {
            if (Keyboard.current[Key.Space].wasPressedThisFrame && UIManager.Instance.gameOver)
            {
                SceneManager.LoadScene(0);
            }
            else if (Keyboard.current[Key.Escape].wasPressedThisFrame && !UIManager.Instance.isQuitting)
            {
                UIManager.Instance.DisplayQuit();
            }
            else if (Keyboard.current[Key.Escape].wasPressedThisFrame && UIManager.Instance.isQuitting)
            {
                UIManager.Instance.RemoveQuit();
                UIManager.Instance.UnPause();
            }
            else if (Keyboard.current[Key.Space].wasPressedThisFrame && UIManager.Instance.isQuitting)
            {
                Application.Quit(0);
                Debug.Log("You leaved the game !");
            }
            else if (Keyboard.current[Key.Space].wasPressedThisFrame && !player2Spawned)
            {
                var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "Player2", pairWithDevice: Keyboard.current);
                player2Spawned = true;
                UIManager.Instance.IsReady(1, true);
                UIManager.Instance.ReplacePlayerInMenu(1, player.transform);
            }
            else if (Keyboard.current[Key.RightShift].wasPressedThisFrame && !player1Spawned)
            {
                var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "Player", pairWithDevice: Keyboard.current);
                player1Spawned = true;
                UIManager.Instance.IsReady(0, true);
                UIManager.Instance.ReplacePlayerInMenu(0, player.transform);
            }
        }

        public void OnDestroy()
        {
            InputSystem.onDeviceChange -= (device, change) => ReadyInput();
            playerInputmanager.onPlayerJoined += ctx => ReadyInput();
            playerInputmanager.onPlayerLeft += ctx => ReadyInput();
        }

    }
}
