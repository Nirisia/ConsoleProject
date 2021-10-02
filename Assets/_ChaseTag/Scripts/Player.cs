///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 27/09/2021 19:08
///-----------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.IsartDigital.ChaseTag.ChaseTag {
	public delegate void PlayerEventHandler(Player player);
	[RequireComponent(typeof(Rigidbody))]
	public class Player : MonoBehaviour {

		public static event PlayerEventHandler OnCollectibleCollected;
		public static event PlayerEventHandler OnMouseCaught;

		[Header("States")]
		[SerializeField] private PlayerSpecs playerSpecs = default;

		private PlayerState currentState = PlayerState.NORMAL;
		public PlayerState CurrentState => currentState;

		private float currentSpeed;

		private Vector3 movementInput;

		new private Rigidbody rigidbody;
		private Vector3 velocity = Vector3.zero;

		// [Header("Inputs")]
		// [SerializeField] private string horizontalInput = "Horizontal";
		// [SerializeField] private string verticalInput = "Vertical";
		// [SerializeField] private KeyCode dashInput = default;

		[Header("Collisions")]
		[SerializeField] private string collectibleTag = "Collectible";
		[SerializeField] private string playerTag = "Player";

		private int numCollectiblesCollected = 0;
		public int NumCollectiblesCollected => numCollectiblesCollected;

		private float currentDash;
		private bool canDash = true;

		private Action doAction;

		private void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();

			SetModeNormal();
			Resume();
		}

		private void Update()
		{
			doAction();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag(collectibleTag))
			{
				numCollectiblesCollected++;
				OnCollectibleCollected?.Invoke(this);
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (currentState == PlayerState.CAT && collision.collider.CompareTag(playerTag))
			{
				Debug.Log("j'ai eu la souris !");
				OnMouseCaught?.Invoke(this);
			}
		}

		#region State Machine
		public void SetModeNormal()
		{
			currentState = PlayerState.NORMAL;

			currentSpeed = playerSpecs.NormalSpeed;
			rigidbody.drag = playerSpecs.NormalDrag;
			currentDash = playerSpecs.NormalDash;
		}

		public void SetModeCat()
		{
			currentState = PlayerState.CAT;

			currentSpeed = playerSpecs.CatSpeed;
			rigidbody.drag = playerSpecs.CatDrag;
			currentDash = playerSpecs.CatDash;
		}

		public void SetModeMouse()
		{
			currentState = PlayerState.MOUSE;

			currentSpeed = playerSpecs.MouseSpeed;
			rigidbody.drag = playerSpecs.MouseDrag;
			currentDash = playerSpecs.MouseDash;
		}

		public void Stop()
		{
			rigidbody.useGravity = false;
			doAction = DoActionStop;
		}

		public void Resume()
		{
			switch (currentState)
			{
				case PlayerState.NORMAL:
					SetModeNormal();
					break;
				case PlayerState.CAT:
					SetModeCat();
					break;
				case PlayerState.MOUSE:
					SetModeMouse();
					break;
			}

			rigidbody.useGravity = true;
			doAction = DoActionNormal;
		}

		#region doAction
		private void DoActionStop() { }

		private void DoActionNormal()
		{
			velocity.x = movementInput.x;
			velocity.z = movementInput.y;

			velocity = velocity.normalized * (currentSpeed * Time.deltaTime);

			rigidbody.AddForce(velocity, ForceMode.VelocityChange);

		}
		#endregion doAction

		#endregion State Machine

		private IEnumerator DashCooldown()
		{
			canDash = false;

			yield return new WaitForSeconds(playerSpecs.DashCooldown);

			canDash = true;
		}

		public void OnMove(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();
		public void OnDash(InputAction.CallbackContext ctx)
		{
			if (canDash)
			{
				rigidbody.AddForce(velocity.normalized * currentDash, ForceMode.Impulse);

				StartCoroutine(DashCooldown());
			}
		}
	}

	public enum PlayerState
    {
		NORMAL,
		CAT,
		MOUSE
    }
}
