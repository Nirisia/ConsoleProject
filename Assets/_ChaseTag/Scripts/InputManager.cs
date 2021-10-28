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
