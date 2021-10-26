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

        [SerializeField] private ParticleSystem fx_explosion;

        [SerializeField] private AudioSource audioSourceExplode = default;

        private float originPlayerSpeed = default;

        private void OnCollisionEnter(Collision collision)
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Wall"))
            {
                StartCoroutine(Explode());
                audioSourceExplode.Play();
            }

            if (other.CompareTag(playerTag))
            {
                Player playerCollided = other.GetComponent<Player>();
                if (!playerCollided.trap)
                {
                    originPlayerSpeed = playerCollided.currentSpeed;
                    playerCollided.currentSpeed = 0f;
                    playerCollided.trap = this;
                    playerCollided.trapCoroutine = StartCoroutine(ReaccelerationPlayer(playerCollided));
                    playerCollided.PlayParticleSlow();
                }
            }
        }

        IEnumerator Explode()
        {
            GetComponentInChildren<Renderer>().enabled = false;
            fx_explosion.Play();

            yield return new WaitForSeconds(2);
            Destroy(transform.parent.gameObject);
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
            player.trap = null;
            yield return null;
        }

        public void CancelTrap(Player player)
        {
            StopCoroutine(player.trapCoroutine);

            player.StopParticleSlow();
            player.trap = null;
        }
    }
}
