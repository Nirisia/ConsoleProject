using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.IsartDigital.ChaseTag
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private Transform player1 = default;
        [SerializeField] private Transform player2 = default;
        [SerializeField] private Camera camMain = default;
        [SerializeField] private Camera cam1 = default;
        [SerializeField] private Camera cam2 = default;
        [SerializeField] private float distanceSwitchCam = 5;

        private bool isSplitScreen = true;

        private void Update()
        {
            float distance = Vector3.Distance(player1.position, player2.position);

            if (distance < distanceSwitchCam && isSplitScreen)
            {
                camMain.enabled = true;
                cam1.enabled = false;
                cam2.enabled = false;
                isSplitScreen = false;

            } else if (distance >= distanceSwitchCam && !isSplitScreen)
            {
                camMain.enabled = false;
                cam1.enabled = true;
                cam2.enabled = true;
                isSplitScreen = true;

                if(player1.position.x < player2.position.x)
                {
                    cam1.rect = new Rect(0,0,0.5f,1);
                    cam2.rect = new Rect(0.5f, 0,1,1);
                }
                else
                {
                    cam1.rect = new Rect(0.5f, 0, 1, 1);
                    cam2.rect = new Rect(0, 0, 0.5f, 1);
                }

            }
        }
    }
}
