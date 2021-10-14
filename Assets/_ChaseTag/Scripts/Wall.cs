using Com.IsartDigital.ChaseTag.ChaseTag;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class Wall : MonoBehaviour
    {

        [SerializeField] private BoxCollider collider_top = default;
        [SerializeField] private BoxCollider collider_bottom = default;
        [SerializeField] private BoxCollider collider_left = default;
        [SerializeField] private BoxCollider collider_right = default;

        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float moveDuration;

        [SerializeField] private ParticleSystem fx_explosion;
        [SerializeField] private ParticleSystem fx_dust;

        [SerializeField] private float playerMagnitudeSpeedForMovingWall = 0.5f;

        [SerializeField] private AudioSource audioSource = default;
        [SerializeField] private AudioClip sfx_Slide = default;
        [SerializeField] private AudioClip sfx_Explode = default;

        private bool isMoving = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player") && !isMoving)
            {
                Player player = collision.collider.GetComponent<Player>();

                if (player.CurrentState == PlayerState.CAT && player.prevSqrMagnitude > playerMagnitudeSpeedForMovingWall)
                {
                    if (collision.GetContact(0).thisCollider == collider_top)
                        StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(-transform.forward)));
                    else if (collision.GetContact(0).thisCollider == collider_bottom)
                        StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(transform.forward)));
                    else if (collision.GetContact(0).thisCollider == collider_left)
                        StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(transform.right)));
                    else if (collision.GetContact(0).thisCollider == collider_right)
                        StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(-transform.right)));

                    fx_dust.Play();

                    audioSource.clip = sfx_Slide;
                    audioSource.Play();
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.collider.CompareTag("Player") && isMoving)
            {
                Player player = collision.collider.GetComponent<Player>();

                if (player.CurrentState == PlayerState.MOUSE)
                {
                    PlayerManager.Instance.ReturnPlayerToNormal();
                    CollectibleManager.Instance.ResetCollectible();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((other.CompareTag("Wall") && !isMoving) || other.CompareTag("Border"))
            {
                StartCoroutine(Explode());
                audioSource.clip = sfx_Explode;
                audioSource.Play();
            } else if (other.CompareTag("Player"))
            {
                other.GetComponent<Player>().RespawnToPosition();
            }
        }

        IEnumerator Explode()
        {
            GetComponentInChildren<Renderer>().enabled = false;
            fx_explosion.Play();
            
            yield return new WaitForSeconds(2);
            Destroy(gameObject);
        }

        IEnumerator AnimateMove(Vector3 origin, Vector3 target)
        {
            isMoving = true;
            float journey = 0f;

            while (journey <= moveDuration)
            {
                journey = journey + Time.deltaTime;
                float percent = Mathf.Clamp01(journey / moveDuration);
                float curvePercent = animationCurve.Evaluate(percent);

                transform.position = Vector3.LerpUnclamped(origin, target, curvePercent);

                yield return null;
            }

            fx_dust.Stop();
            isMoving = false;
        }
    }
}
