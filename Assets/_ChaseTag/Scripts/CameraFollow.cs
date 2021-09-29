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
        [SerializeField] private float heightMultiplicator = 0.1f;
        [SerializeField] private float widthMultiplicator = 0.1f;
        [SerializeField] private float smoothTime = 0.5F;
        [SerializeField] private float lookSpeed = 5;
        [SerializeField] private Vector3 maxQuaternionRota;
        [SerializeField] private Vector3 minQuaternionRota;
        [SerializeField] private bool isLookat = true;

        private Vector3 velocity = Vector3.zero;

        private void Update()
        {
            MoveCamera();
        }

        private void MoveCamera()
        {
            Vector3 newPos = target.position;
            float distance = Vector3.Distance(target.position, target2.position);
            newPos = (target.position + target2.position) / 2;
            
            Vector3 targetPosition = new Vector3(newPos.x, newPos.y + offsetY + (distance * heightMultiplicator), newPos.z + offsetZ + (distance * widthMultiplicator));
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            if (isLookat) { 
                Quaternion targetRotation = Quaternion.LookRotation(newPos - transform.position);
                targetRotation = ClampRotation(targetRotation, minQuaternionRota.x, maxQuaternionRota.x, minQuaternionRota.y, maxQuaternionRota.y, minQuaternionRota.z, maxQuaternionRota.z);
                
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * Time.deltaTime);
            }
        }

        private Quaternion ClampRotation(Quaternion targetRotation, float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            if (targetRotation.x > maxX)
                targetRotation.x = maxX;
            else if (targetRotation.x < minX)
                targetRotation.x = minX;

            if (targetRotation.y > maxY)
                targetRotation.y = maxY;
            else if (targetRotation.y < minY)
                targetRotation.y = minY;

            if (targetRotation.z > maxZ)
                targetRotation.z = maxZ;
            else if (targetRotation.z < minZ)
                targetRotation.z = minZ;

            return targetRotation;
        }
    }
}