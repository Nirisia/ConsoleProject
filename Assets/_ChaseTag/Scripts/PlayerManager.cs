///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 29/09/2021 18:05
///-----------------------------------------------------------------

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public sealed class PlayerManager : MonoBehaviour {
	
		public static PlayerManager Instance { get; private set; }

        [Serializable]
        public class PlayerInfo
        {
            [SerializeField] public Player player = null;
            [SerializeField] public Color color = default;
            [SerializeField] public Gradient gradient = default;
            [SerializeField] public Text txtState = default;
            [SerializeField] public bool isReady = false;
            [SerializeField] public Vector3 spawnPosition = default;
            [SerializeField] public int score = default;
        }

        public int playerCount = 0;
        public PlayerInfo[] playerInfos;

        [Header("State name")]
        [SerializeField] private string txtStateNormal = "GET ITEM FIRST";
        [SerializeField] private string txtStateCat = "CAT";
        [SerializeField] private string txtStateMouse = "MOUSE";
        //[SerializeField] private Text txtCollectiblePlayer1 = default;
        //[SerializeField] private Text txtCollectiblePlayer2 = default;

        [SerializeField] private Vector3[] podiumPosition = new Vector3[4];


        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogWarning("Trying to create multiple instances of singleton script, creation denied");
                Destroy(gameObject);
            }

            Player.OnCollectibleCollected += Player_OnCollectibleCollected;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            Player.OnCollectibleCollected -= Player_OnCollectibleCollected;
        }

        public void ReturnPlayerToNormal()
        {
            for (int i = 0; i < playerCount; i++)
            {
                playerInfos[i].player.haveCrown = false;
                playerInfos[i].player.SetModeNormal();
                
            }
            Player_OnCollectibleCollected();
        }
 
        private void Player_OnCollectibleCollected(Player _ = default)
        {
            for (int i = 0; i < playerCount; i++)
            {
                var player = playerInfos[i].player;
                if (player.haveCrown)
                {
                    player.SetModeMouse();
                    player.SetSize(PlayerState.MOUSE);
                    playerInfos[i].txtState.text = txtStateMouse;
                }
                else
                {
                    player.SetModeCat();
                    player.SetSize(PlayerState.CAT);
                    playerInfos[i].txtState.text = txtStateCat;

                }
                
            }
        }

        public bool TryGetMousePlayer(out Player player)
        {
            for (int i = 0; i < playerCount; i++)
            {
                var playerI = playerInfos[i].player;
                if (playerI.haveCrown)
                {
                    player = playerI;
                    return true;
                }
            }

            player = null;
            return false;
        }

        public void DestroyPlayer()
        {
            for (int i = 0; i < playerCount; i++)
            {
                Destroy(playerInfos[i].player.gameObject);
            }
        }

        public int GetPlayerId(Player player)
        {
            for (int i = 0; i < playerCount; i++)
            {
                if (player == playerInfos[i].player)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool TryGetPlayerWithMostElapsedTime(out Player player)
        {
            player = playerInfos[0].player;
            for (int i = 1; i < playerCount; i++)
            {
                if (playerInfos[i].player.MouseElapsedTime > player.MouseElapsedTime)
                    player = playerInfos[i].player;
            }

            if (player.MouseElapsedTime == 0)
                return false;

            return true;
        }

        public float GetPlayerMouseElapsedTime(int playerId)
        {
            return playerInfos[playerId].player.MouseElapsedTime;
        }
        
        public bool AllPlayersReady()
        {
            if (playerCount <= 1) return false;
            
            for (int i = 0; i < playerCount; i++)
            {
                if (!playerInfos[i].isReady)
                    return false;
            }

            return true;
        }

        public void SetPlayersControlScheme(string actionMap)
        {
            for (int i = 0; i < playerCount; i++)
            {
                playerInfos[i].player.GetComponent<PlayerInput>().SwitchCurrentActionMap(actionMap);
            }
        }

        public void PlayerJoined(int playerID)
        {
            playerCount++;
            playerInfos[playerID].player.GetComponentInChildren<Renderer>().material.color = playerInfos[playerID].color;
        }

        public void ReplacePlayer(int id, Transform player)
        {
            if (id > playerInfos.Length) return;

            player.position = playerInfos[id].spawnPosition;
            playerInfos[id].player.Stop();
            UIManager.Instance.DisplaySpawnEffect(id);
        }

        public void ReplaceAllPlayer()
        {
            for (int i = 0; i < playerCount; i++)
            {
                ReplacePlayer(i, playerInfos[i].player.transform);
                playerInfos[i].player.ResetPlayer();
            }
        }

        public void ResetAllPlayer()
        {
            for (int i = 0; i < playerCount; i++)
            {
                playerInfos[i].player.ResetPlayer();
            }
        }

        public void setOnPodium()
        {
            PlayerInfo[] leaderboard = new PlayerInfo[playerCount];

            for (int i = 0; i < playerCount; i++)
            {
                leaderboard[i] = playerInfos[i];
            }
            
            leaderboard = leaderboard.OrderBy(x => x.score).ToArray();

            leaderboard[0].player.transform.position = podiumPosition[3];
            CameraFollow.Instance.targets.Remove(leaderboard[0].player.transform);
            leaderboard[0].player.ResetPlayer();

            var countPodiumPod = 0;

            for (int i = leaderboard.Length - 1; i >= 1; i--)
            {
                leaderboard[i].player.PlayCoroutineStun();
                leaderboard[i].player.transform.position = podiumPosition[countPodiumPod];
                leaderboard[i].player.ResetPlayer();
                countPodiumPod++;
            }
        }

        public void HidePlayer()
        {
            for (int i = 0; i < playerCount; i++)
            {
                playerInfos[i].player.GetComponentInChildren<Renderer>().enabled = false;
            }
        }

        public void ShowPlayer()
        {
            for (int i = 0; i < playerCount; i++)
            {
                playerInfos[i].player.GetComponentInChildren<Renderer>().enabled = true;
            }
        }
    }
}
