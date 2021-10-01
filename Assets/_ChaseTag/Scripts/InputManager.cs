using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.IsartDigital.ChaseTag
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputmanager = default;
        [SerializeField] private GameObject playerPrefab = default;

        private bool player1Spawned = false;
        private bool player2Spawned = false;

        private void Start()
        {

            #if UNITY_EDITOR
                playerInputmanager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
            #else
                InputSystem.onDeviceChange += (device, change) => ReadyInput();
                playerInputmanager.onPlayerJoined += ctx => ReadyInput();
                playerInputmanager.onPlayerLeft += ctx => ReadyInput();
            #endif
        }

        private void Update()
        {
#if UNITY_EDITOR

                if (Keyboard.current[Key.Space].wasPressedThisFrame && !player2Spawned)
                {
                    PlayerInput.Instantiate(playerPrefab, controlScheme: "Player2", pairWithDevice: Keyboard.current);
                    player2Spawned = true;
                    UIManager.Instance.IsReady(1, true);
                } 
                else if (Keyboard.current[Key.Numpad0].wasPressedThisFrame && !player1Spawned)
                {
                    PlayerInput.Instantiate(playerPrefab, controlScheme: "Player", pairWithDevice: Keyboard.current);
                    player1Spawned = true;
                    UIManager.Instance.IsReady(0, true);
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

        public void OnDestroy()
        {
            InputSystem.onDeviceChange -= (device, change) => ReadyInput();
            playerInputmanager.onPlayerJoined += ctx => ReadyInput();
            playerInputmanager.onPlayerLeft += ctx => ReadyInput();
        }

    }
}
