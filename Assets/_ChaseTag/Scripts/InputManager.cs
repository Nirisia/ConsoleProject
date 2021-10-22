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
            PlayerManager.Instance.playerInfos[playerInput.playerIndex].player = playerInput.gameObject.GetComponent<Player>();
            PlayerManager.Instance.PlayerJoined(playerInput.playerIndex);
            UIManager.Instance.ReplacePlayerInMenu(playerInput.playerIndex, playerInput.gameObject.transform);
        }

        public void OnPlayerLeft(PlayerInput playerInput)
        {
            Debug.Log("Player Left");
            PlayerManager.Instance.playerCount--;
        }

        public void OnDestroy()
        {
            playerInputmanager.onPlayerJoined -= OnPlayerJoined;
            playerInputmanager.onPlayerLeft -= OnPlayerLeft;
        }

    }
}
