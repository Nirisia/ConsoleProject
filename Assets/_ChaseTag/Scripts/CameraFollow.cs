using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target = default;
        [SerializeField] private Transform target2 = default;
        [SerializeField] private float offsetZ = -2;
        [SerializeField] private float offsetY = 5;
        [SerializeField] private float smoothTime = 0.5F;
        [SerializeField] private float lookSpeed = 5;
        [SerializeField] private float maxQuaternionRotaY = 0.1f;

        private Vector3 velocity = Vector3.zero;

        private void Update()
        {
            MoveCamera();
        }

        private void MoveCamera()
        {
            Vector3 newPos = target.position;

            if (target2 != null)
                newPos = (target.position + target2.position) / 2;
            
            Vector3 targetPosition = new Vector3(newPos.x, newPos.y + offsetY, newPos.z + offsetZ);
            Quaternion targetRotation = Quaternion.LookRotation(newPos - transform.position);

            targetRotation = ClampRotation(targetRotation, targetRotation.x, maxQuaternionRotaY, 0);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
        }

        private Quaternion ClampRotation(Quaternion targetRotation, float x, float y, float z)
        {
            if (targetRotation.x > x)
                targetRotation.x = x;
            else if (targetRotation.x < -x)
                targetRotation.x = -x;

            if (targetRotation.y > y)
                targetRotation.y = y;
            else if (targetRotation.y < -y)
                targetRotation.y = -y;

            if (targetRotation.z > z)
                targetRotation.z = z;
            else if (targetRotation.z < -z)
                targetRotation.z = -z;

            return targetRotation;
        }
    }
}
