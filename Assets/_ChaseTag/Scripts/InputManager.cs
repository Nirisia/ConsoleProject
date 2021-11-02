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
using UnityEngine.InputSystem.HID;

namespace Com.IsartDigital.ChaseTag
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInputManager playerInputmanager = default;
        [SerializeField] private GameObject playerPrefab = default;

        private class NpadData
        {
            public NpadData(NpadId npadId)
            {
                this.npadId = npadId;
            }

            public NpadId npadId;
            public NpadStyle previousNpadStyle = NpadStyle.Invalid;
            public VibrationDeviceHandle? m_vibrationDeviceHandleLeft = null;
            public VibrationDeviceHandle? m_vibrationDeviceHandleRight = null;
        }

        private NpadData[] npadDatas =
        {
            new NpadData(NpadId.No1),
            new NpadData(NpadId.No2),
            new NpadData(NpadId.No3),
            new NpadData(NpadId.No4),
        };

        private void Start()
        {
            playerInputmanager.onPlayerJoined += OnPlayerJoined;
            playerInputmanager.onPlayerLeft += OnPlayerLeft;

#if UNITY_SWITCH
            Npad.Initialize(); 
            Npad.SetSupportedStyleSet(NpadStyle.JoyLeft | NpadStyle.JoyRight);
            NpadJoy.SetHoldType(NpadJoyHoldType.Horizontal);
            NpadJoy.SetHandheldActivationMode(NpadHandheldActivationMode.None);
            Npad.SetSupportedIdType(npadIds);

            ControllerSupportArg controllerSupportArgs = new ControllerSupportArg();
            controllerSupportArgs.SetDefault();

            controllerSupportArgs.playerCountMax = 4;
            controllerSupportArgs.playerCountMin = 2;

#if !UNITY_EDITOR

            UnityEngine.Switch.Applet.Begin(); //call before calling a system applet to stop all Unity threads (including audio and networking)

            nn.hid.ControllerSupport.Show(controllerSupportArgs);

            UnityEngine.Switch.Applet.End();
#endif
#endif
        }

        private void Update()
        {
            if (playerInputmanager.playerCount == playerInputmanager.maxPlayerCount)
            {
                playerInputmanager.DisableJoining();
            }

#if UNITY_SWITCH
            for(int i = 0; i < npadDatas.Length; i++)
            {
                UpdateVibrationDeviceHandles(ref npadDatas[i]);
            }
#endif
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

        private void UpdateVibrationDeviceHandles(ref NpadData data)
        {
            // Assumes Npad initialization has already happened
            NpadStyle currentStyle = Npad.GetStyleSet(data.npadId);

            if (currentStyle == data.previousNpadStyle)
            {
                return;
            }

            data.previousNpadStyle = currentStyle;

            // We should check if a device goes from inactive to active so we can clear any vibration it's still outputting from when it became inactive.
            bool wasLeftDeviceNull = (data.m_vibrationDeviceHandleLeft == null);
            bool wasRightDeviceNull = (data.m_vibrationDeviceHandleRight == null);

            // Set these as null. By the end of the function, any active vibration devices will have a set handle.
            data.m_vibrationDeviceHandleLeft = null;
            data.m_vibrationDeviceHandleRight = null;

            if (currentStyle == NpadStyle.None || currentStyle == NpadStyle.Invalid)
            {
                // Invalid or disconnected controller.
                return;
            }

            VibrationDeviceHandle[]
                vibrationDeviceHandles =
                    new VibrationDeviceHandle[2]; // Temporary buffer to get handles
            int vibrationDeviceCount = Vibration.GetDeviceHandles(vibrationDeviceHandles, 2,
                data.npadId, currentStyle);

            VibrationDeviceHandle?
                vibrationDeviceHandleLeft = null;
            VibrationDeviceHandle? vibrationDeviceHandleRight = null;

            for (int i = 0; i < vibrationDeviceCount; i++)
            {
                Vibration.InitializeDevice(vibrationDeviceHandles[i]);

                VibrationDeviceInfo
                    vibrationDeviceInfo =
                        new VibrationDeviceInfo(); // This is basically just used as an 'out' parameter for GetDeviceInfo
                Vibration.GetDeviceInfo(ref vibrationDeviceInfo, vibrationDeviceHandles[i]);

                // Cache references to our device handles.
                switch (vibrationDeviceInfo.position)
                {
                    case VibrationDevicePosition.Left:
                        data.m_vibrationDeviceHandleLeft = vibrationDeviceHandles[i];
                        if (wasLeftDeviceNull)
                        {
                            if (data.m_vibrationDeviceHandleLeft != null)
                            {
                                Vibration.SendValue(data.m_vibrationDeviceHandleLeft.Value, new VibrationValue());
                            }
                        }

                        break;

                    case VibrationDevicePosition.Right:
                        data.m_vibrationDeviceHandleRight = vibrationDeviceHandles[i];
                        if (wasRightDeviceNull)
                        {
                            if (data.m_vibrationDeviceHandleLeft != null)
                            {
                                Vibration.SendValue(data.m_vibrationDeviceHandleLeft.Value, new VibrationValue());
                            }
                        }

                        break;

                    default: // This should never happen
                        Debug.Assert(false, "Invalid VibrationDevicePosition specified");
                        break;
                }
            }
        }
    }
}