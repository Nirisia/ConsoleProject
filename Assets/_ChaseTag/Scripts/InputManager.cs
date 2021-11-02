using System;
using Com.IsartDigital.ChaseTag.ChaseTag;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

using nn.hid;
using TMPro;

namespace Com.IsartDigital.ChaseTag
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputmanager = default;
        [SerializeField] private GameObject playerPrefab = default;
        
        private void Start()
        {
            playerInputmanager.onPlayerJoined += OnPlayerJoined;
            playerInputmanager.onPlayerLeft += OnPlayerLeft;

#if UNITY_SWITCH && !UNITY_EDITOR
            Npad.Initialize(); 
            Npad.SetSupportedStyleSet(NpadStyle.JoyLeft | NpadStyle.JoyRight);
            NpadJoy.SetHoldType(NpadJoyHoldType.Horizontal);
            NpadJoy.SetHandheldActivationMode(NpadHandheldActivationMode.None);
            NpadId[] npadIds = { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };
            Npad.SetSupportedIdType(npadIds);

            ControllerSupportArg controllerSupportArgs = new ControllerSupportArg();
            controllerSupportArgs.SetDefault();

            controllerSupportArgs.playerCountMax = 4;
            controllerSupportArgs.playerCountMin = 2;

            UnityEngine.Switch.Applet.Begin(); //call before calling a system applet to stop all Unity threads (including audio and networking)

            nn.hid.ControllerSupport.Show(controllerSupportArgs);

            UnityEngine.Switch.Applet.End();

#endif
        }

        private void Update()
        {
            if (playerInputmanager.playerCount == playerInputmanager.maxPlayerCount)
            {
                playerInputmanager.DisableJoining();
            }
        }

        public void OnPlayerJoined(PlayerInput playerInput)
        {
            Debug.Log("Player Joined");
            Player myPlayer = playerInput.gameObject.GetComponent<Player>();

            PlayerManager.Instance.playerInfos[playerInput.playerIndex].player = myPlayer;
            PlayerManager.Instance.PlayerJoined(playerInput.playerIndex);
            PlayerManager.Instance.ReplacePlayer(playerInput.playerIndex, playerInput.gameObject.transform);
            CameraFollow.Instance.targets.Add(myPlayer.transform);
        }

        public void OnPlayerLeft(PlayerInput playerInput)
        {
            Debug.Log("Player Left");
            Player myPlayer = playerInput.gameObject.GetComponent<Player>();

            PlayerManager.Instance.playerCount--;

            CameraFollow.Instance.targets.Remove(myPlayer.transform);
        }

        public void OnDestroy()
        {
            playerInputmanager.onPlayerJoined -= OnPlayerJoined;
            playerInputmanager.onPlayerLeft -= OnPlayerLeft;
        }

    }
}
