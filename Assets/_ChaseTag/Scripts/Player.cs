///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 27/09/2021 19:08
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public class Player : MonoBehaviour {

		[SerializeField] private string horizontalInput = "Horizontal";
		[SerializeField] private string verticalInput = "Vertical";

		[SerializeField] private PlayerSpecs playerSpecs = default;

		private float currentSpeed;

		private Vector3 velocity = Vector3.zero;

        private void Awake()
        {
			currentSpeed = playerSpecs.NormalSpeed;
        }

        private void Update()
        {
			velocity.x = Input.GetAxis(horizontalInput);
			velocity.z = Input.GetAxis(verticalInput);

			velocity = velocity.normalized * (currentSpeed *Time.deltaTime);

			transform.position += velocity;
        }
    }
}
