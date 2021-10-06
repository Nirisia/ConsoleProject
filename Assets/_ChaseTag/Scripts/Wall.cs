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

        private bool isMoving = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (!isMoving)
            {
                if (collision.GetContact(0).thisCollider == collider_top)
                    StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(-transform.forward)));
                else if (collision.GetContact(0).thisCollider == collider_bottom)
                    StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(transform.forward)));
                else if (collision.GetContact(0).thisCollider == collider_left)
                    StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(transform.right)));
                else if (collision.GetContact(0).thisCollider == collider_right)
                    StartCoroutine(AnimateMove(transform.position, transform.TransformPoint(-transform.right)));
            }
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

            isMoving = false;
        }
    }
}