///-----------------------------------------------------------------
/// Author : Erwann COURTY
/// Date : 02/10/2021 17:13
///-----------------------------------------------------------------

using Com.IsartDigital.ChaseTag.ChaseTag;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class Trap : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float duration = default;


        [Header("Collision")]
        [SerializeField] private string playerTag = "Player";

        private float originPlayerSpeed = default;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag(playerTag))
            {
                Player playerCollided = collision.collider.GetComponent<Player>();
                originPlayerSpeed = playerCollided.currentSpeed;
                playerCollided.currentSpeed = 0f;
                StartCoroutine(ReaccelerationPlayer(playerCollided));
                playerCollided.PlayParticleSlow();
            }
        }

        private IEnumerator ReaccelerationPlayer(Player player) 
        {
            float elapsedTime = 0f;
            float actualSpeedPlayer = player.currentSpeed;

            while (elapsedTime < duration)
            {
                player.currentSpeed = Mathf.Lerp(actualSpeedPlayer, originPlayerSpeed, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            player.StopParticleSlow();
            player.currentSpeed = originPlayerSpeed;
            yield return null;
            
        }
    }
}
