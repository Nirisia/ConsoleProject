///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 27/09/2021 19:08
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public delegate void PlayerEventHandler(Player player);
	[RequireComponent(typeof(Rigidbody))]
	public class Player : MonoBehaviour {

		[Header("States")]
		[SerializeField] private PlayerSpecs playerSpecs = default;
		
		private PlayerState currentState = PlayerState.NORMAL;
		public PlayerState CurrentState => currentState;
		
		private float currentSpeed;
		
		new private Rigidbody rigidbody;
		private Vector3 velocity = Vector3.zero;

		[Header("Inputs")]
		[SerializeField] private string horizontalInput = "Horizontal";
		[SerializeField] private string verticalInput = "Vertical";

        private void Awake()
        {
			rigidbody = GetComponent<Rigidbody>();

			SetModeNormal();
        }

        private void Update()
        {
			velocity.x = Input.GetAxis(horizontalInput);
			velocity.z = Input.GetAxis(verticalInput);

			velocity = velocity.normalized * (currentSpeed *Time.deltaTime);

			rigidbody.AddForce(velocity, ForceMode.VelocityChange);
        }

        #region State Machine
		public void SetModeNormal()
        {
			currentState = PlayerState.NORMAL;

			currentSpeed = playerSpecs.NormalSpeed;
			rigidbody.drag = playerSpecs.NormalDrag;
        }

		public void SetModeCat()
        {
			currentState = PlayerState.CAT;

			currentSpeed = playerSpecs.CatSpeed;
			rigidbody.drag = playerSpecs.CatDrag;
        }

		public void SetModeMouse()
        {
			currentState = PlayerState.MOUSE;

			currentSpeed = playerSpecs.MouseSpeed;
			rigidbody.drag = playerSpecs.MouseDrag;
        }
        #endregion State Machine
    }

	public enum PlayerState
    {
		NORMAL,
		CAT,
		MOUSE
    }
}
