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

        [SerializeField] private AnimationCurve animationCurveFall;

        private float originPlayerSpeed = default;

        private float fallDuration = 2;

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

                    if (playerCollided.trapCoroutine == null)
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

            player.trapCoroutine = null;

            yield return null;


        }

        public void CancelTrap(Player player)
        {
            StopCoroutine(player.trapCoroutine);

            player.trapCoroutine = null;
            player.StopParticleSlow();
            player.trap = null;
        }

        IEnumerator AnimateFall(Vector3 origin, Vector3 target)
        {
            float journey = 0f;

            while (journey <= fallDuration)
            {
                journey = journey + Time.deltaTime;
                float percent = Mathf.Clamp01(journey / fallDuration);
                float curvePercent = animationCurveFall.Evaluate(percent);

                transform.parent.position = Vector3.LerpUnclamped(origin, target, curvePercent);

                yield return null;
            }
        }

        public void Fall()
        {
            Vector3 groundPos = transform.parent.position;

            groundPos.y = 0f;

            StartCoroutine(AnimateFall(transform.parent.position, groundPos));
        }
    }
}
