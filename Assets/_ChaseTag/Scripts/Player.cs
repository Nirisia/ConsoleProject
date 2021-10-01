///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 27/09/2021 19:08
///-----------------------------------------------------------------

using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	[RequireComponent(typeof(Rigidbody))]
	public class Player : MonoBehaviour {

		[SerializeField] private PlayerSpecs playerSpecs = default;

		private float currentSpeed;
		private Vector2 movementInput;
		private bool isDash = false;
		private bool isGameStart = false;

		private Vector3 velocity = Vector3.zero;

		new private Rigidbody rigidbody;

        private void Awake()
        {
			rigidbody = GetComponent<Rigidbody>();

			currentSpeed = playerSpecs.NormalSpeed;
			rigidbody.drag = playerSpecs.NormalDrag;
			rigidbody.useGravity = false;
		}

        private void Update()
        {
			MovePlayer();
        }

		public void StartMoving()
        {
			isGameStart = true;
			rigidbody.useGravity = true;
        }

		private void MovePlayer()
        {
			if (isGameStart)
            {
				velocity.x = movementInput.x;
				velocity.z = movementInput.y;

				velocity = velocity.normalized * (currentSpeed * Time.deltaTime);

				rigidbody.AddForce(velocity, ForceMode.VelocityChange);
			}
        }

		public void OnMove(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();
		public void OnDash(InputAction.CallbackContext ctx) => isDash = ctx.action.triggered;
    }
}
