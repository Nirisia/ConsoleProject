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
        public static InputManager Instance { get; private set; }

        [SerializeField] private PlayerInputManager playerInputmanager = default;
        [SerializeField] private GameObject playerPrefab = default;

        public List<string> testNpad;

        private class NpadData
        {
            public NpadData(NpadId npadId)
            {
                this.npadId = npadId;
            }

            public NpadId npadId;
            public NpadStyle previousNpadStyle = NpadStyle.Invalid;
            public VibrationDeviceHandle? vibrationDeviceHandle = null;
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

            NpadId[] npadIds = { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };
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

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("Trying to create multiple instances of singleton script, creation denied");
                Destroy(gameObject);
            }
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

            GetNpadID(playerInput);

            Debug.LogError(playerInput.devices[0].name);
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
            if (Instance == this)
                Instance = null;

            playerInputmanager.onPlayerJoined -= OnPlayerJoined;
            playerInputmanager.onPlayerLeft -= OnPlayerLeft;
        }

        private void GetNpadID(PlayerInput playerInput)
        {
            switch (playerInput.devices[0].name)
            {
                case "NPad1":
                    PlayerManager.Instance.playerInfos[playerInput.playerIndex].player.npadID = 0;
                    break;

                case "NPad2":
                    PlayerManager.Instance.playerInfos[playerInput.playerIndex].player.npadID = 1;
                    break;

                case "NPad3":
                    PlayerManager.Instance.playerInfos[playerInput.playerIndex].player.npadID = 2;
                    break;

                case "NPad4":
                    PlayerManager.Instance.playerInfos[playerInput.playerIndex].player.npadID = 3;
                    break;

                default:
                    break;
            }
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
            bool wasLeftDeviceNull = (data.vibrationDeviceHandle == null);

            // Set these as null. By the end of the function, any active vibration devices will have a set handle.
            data.vibrationDeviceHandle = null;

            if (currentStyle == NpadStyle.None || currentStyle == NpadStyle.Invalid)
            {
                // Invalid or disconnected controller.
                return;
            }

            VibrationDeviceHandle[]
                vibrationDeviceHandles =
                    new VibrationDeviceHandle[1]; // Temporary buffer to get handles
            int vibrationDeviceCount = Vibration.GetDeviceHandles(vibrationDeviceHandles, 1,
                data.npadId, currentStyle);

            for (int i = 0; i < vibrationDeviceCount; i++)
            {
                Vibration.InitializeDevice(vibrationDeviceHandles[i]);

                data.vibrationDeviceHandle = vibrationDeviceHandles[i];

                VibrationDeviceHandle deviceHandle = data.vibrationDeviceHandle.Value;

                Vibration.SendValue(deviceHandle, new VibrationValue(0.0f, 0.0f, 0.0f, 0.0f));
            }
        }

        public void SetVibration(float lowAmplitude, float lowFrequency, float highAmplitude, float highFrequency, int npadIndex)
        {
            if (npadIndex < 0 || npadIndex >= npadDatas.Length) return;

            UpdateVibrationDeviceHandles(ref npadDatas[npadIndex]);
#if UNITY_SWITCH
            VibrationDeviceHandle? vibrationDeviceHandle = npadDatas[npadIndex].vibrationDeviceHandle;

            if (!vibrationDeviceHandle.HasValue) return;

            Vibration.SendValue(vibrationDeviceHandle.Value, new VibrationValue(lowAmplitude, lowFrequency, highAmplitude, highFrequency));
#endif
        }

        public IEnumerator SetVibrationSeconds(float lowAmplitude, float lowFrequency, float highAmplitude, float highFrequency, int npadIndex, float time)
        {
            InputManager.Instance.SetVibration(lowAmplitude, lowFrequency, highAmplitude, highFrequency, npadIndex);
            yield return new WaitForSeconds(time);
            InputManager.Instance.SetVibration(0f, 0f, 0f, 0f, npadIndex);
        }

        public IEnumerator PlayVibrationFile(TextAsset text, int npadIndex)
        {
            byte[] bytes = text.bytes;

            VibrationFileParserContext vibrationFileParserContext = new VibrationFileParserContext();
            VibrationFileInfo vibrationFileInfo = new VibrationFileInfo();
            VibrationFile.Parse(ref vibrationFileInfo, ref vibrationFileParserContext, bytes, bytes.Length);

            int pos = 0;
            while (pos < vibrationFileInfo.sampleLength)
            {
                VibrationValue vibrationValue = new VibrationValue();
                VibrationFile.RetrieveValue(ref vibrationValue, pos, ref vibrationFileParserContext);
                SetVibration(vibrationValue.amplitudeLow,
                    vibrationValue.frequencyLow,
                    vibrationValue.amplitudeHigh,
                    vibrationValue.frequencyHigh,
                    npadIndex
                   );
                pos++;
                yield return null;
            }
            SetVibration(0f, 0f, 0f, 0f, npadIndex);
        }
    }
}