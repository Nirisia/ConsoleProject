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

		[HideInInspector] public float currentSpeed;

		private Vector3 movementInput;

		new private Rigidbody rigidbody;
		private Vector3 velocity = Vector3.zero;

		// [Header("Inputs")]
		// [SerializeField] private string horizontalInput = "Horizontal";
		// [SerializeField] private string verticalInput = "Vertical";
		// [SerializeField] private KeyCode dashInput = default;
		[Header("Sound")]
		[SerializeField] private AudioSource audioSource = default;
		[SerializeField] private AudioClip[] sounds_dash = default;
		[SerializeField] private AudioClip[] sounds_collision = default;

		[Header("Collisions")]
		[SerializeField] private string collectibleTag = "Collectible";
		[SerializeField] private string playerTag = "Player";
		[SerializeField] private string wallTag = "Wall";
		[SerializeField] private string borderTag = "Border";
		[SerializeField,Range(0.1f,4f)] private float secondStunAfterCollision = default;
		[SerializeField] private Vector3 respawnPosition;

		[Header("Particles")]
		[SerializeField] private ParticleSystem fx_Slow = default;
		[SerializeField] private ParticleSystem fx_Dash = default;
		[SerializeField] private ParticleSystem fx_Explosion = default;
		[SerializeField] private ParticleSystem fx_Shield = default;
		[SerializeField] private ParticleSystem fx_StolenCrown = default;
		[SerializeField] private ParticleSystem fx_Crown = default;
		[SerializeField] private TrailRenderer fx_CrownTrail = default;
		[SerializeField] private ParticleSystemForceField fx_particleAttractor = default;

		[SerializeField] private Renderer playerRenderer = default;
		[SerializeField] private SpriteRenderer spriteCrown = default;

		[SerializeField] private Vector3 sizeNormal;
		[SerializeField] private Vector3 sizeCat;
		[SerializeField] private Vector3 sizeMouse;

		[SerializeField] private float rescaleDuration = 1f;
		[SerializeField] private AnimationCurve rescaleAnimation;


		private CameraShake cameraShake = default;

		private Coroutine stunCoroutine;

		//private int numCollectiblesCollected = 0;
		//public int NumCollectiblesCollected => numCollectiblesCollected;

		public bool haveCrown = false;

		private float currentDash;
		private bool canDash = true;

		public float prevSqrMagnitude;

		private Action doAction;

		public bool isTrapped = false;

		public float MouseElapsedTime { get; private set; } = 0f;

		private void Awake()
		{
			rigidbody = GetComponent<Rigidbody>();

			SetModeVoid();
			Resume();
			audioSource.clip = sounds_collision[1];
			audioSource.Play();

			cameraShake = Camera.main.GetComponentInParent<CameraShake>();
		}

		private void Update()
		{
			doAction();
			KeepCrownInPlace();
		}

		void FixedUpdate()
		{
			prevSqrMagnitude = rigidbody.velocity.sqrMagnitude;
		}

		private void KeepCrownInPlace()
		{
			var newPos = transform.position;
			newPos.y = 1f;

			spriteCrown.transform.position = newPos;
			spriteCrown.transform.LookAt(CameraFollow.Instance.transform);

			var rota = spriteCrown.transform.rotation;
			rota.x = 0;

			spriteCrown.transform.rotation = rota;
		}

		public void RespawnToPosition()
        {
			Explode();
			GetComponentInChildren<TrailRenderer>().emitting = false;
			transform.position = respawnPosition;
			GetComponentInChildren<TrailRenderer>().emitting = true;
		}

		public void PlayParticleSlow()
        {
			fx_Slow.Play();
        }
		
		public void StopParticleSlow()
        {
			fx_Slow.Stop();
        }

		public void Explode()
        {
			ParticleSystem.MainModule main = fx_Explosion.main;
			main.startColor = playerRenderer.material.color;

			fx_Explosion.Play();
			//playerRenderer.enabled = false;
			//rigidbody.useGravity = false;
			//rigidbody.isKinematic = true;
			//GetComponent<SphereCollider>().enabled = false;
		}

		public void SetSize(PlayerState state)
        {
            switch (state)
            {
                case PlayerState.NORMAL:
					StartCoroutine(AnimateScale(transform.localScale, sizeNormal));
                    break;
                case PlayerState.CAT:
					StartCoroutine(AnimateScale(transform.localScale, sizeCat));
					break;
                case PlayerState.MOUSE:
					StartCoroutine(AnimateScale(transform.localScale, sizeMouse));
					break;
                default:
                    break;
            }
        }

        
		public void ChangeCrown()
        {
			haveCrown = !haveCrown;
			if (!haveCrown)
				stunCoroutine = StartCoroutine(StunAfterCollision());
			else
				AddCollectible();
		}

        public void AddCollectible()
        {
            OnCollectibleCollected?.Invoke(this);
        }

        private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag(collectibleTag))
			{
				ChangeCrown();
				fx_Crown.Play();
				fx_CrownTrail.enabled = true;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (collision.collider.CompareTag(playerTag) || collision.collider.CompareTag(wallTag) || collision.collider.CompareTag(borderTag))
			{
				cameraShake.enabled = true;
				audioSource.clip = sounds_collision[0];
				audioSource.Play();
			}

			if (currentState == PlayerState.CAT && collision.collider.CompareTag(playerTag) && stunCoroutine == null)
			{
				Debug.Log("j'ai eu la souris !");
				//OnMouseCaught?.Invoke(this);

				Player playerCollided = collision.gameObject.GetComponent<Player>();
				
				playerCollided.ChangeCrown();
				ChangeCrown();

				fx_particleAttractor.gameObject.SetActive(true);
				fx_Crown.Play();
				fx_CrownTrail.enabled = true;

				playerCollided.fx_StolenCrown.Play();
				playerCollided.fx_particleAttractor.gameObject.SetActive(false);
				playerCollided.fx_Crown.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
				playerCollided.fx_CrownTrail.enabled = false;

				ParticleSystem.MainModule main = fx_Shield.main;
				main.duration = secondStunAfterCollision;

				fx_Shield.Play();

				audioSource.clip = sounds_collision[1];
				audioSource.Play();
			}
		}

		#region State Machine
		public void SetModeNormal()
		{
			currentState = PlayerState.NORMAL;

			currentSpeed = playerSpecs.NormalSpeed;
			rigidbody.drag = playerSpecs.NormalDrag;
			currentDash = playerSpecs.NormalDash;
			spriteCrown.enabled = false;
		}

		public void SetModeCat()
		{
			currentState = PlayerState.CAT;

			currentSpeed = playerSpecs.CatSpeed;
			rigidbody.drag = playerSpecs.CatDrag;
			currentDash = playerSpecs.CatDash;
			spriteCrown.enabled = false;
		}

		public void SetModeMouse()
		{
			currentState = PlayerState.MOUSE;

			currentSpeed = playerSpecs.MouseSpeed;
			rigidbody.drag = playerSpecs.MouseDrag;
			currentDash = playerSpecs.MouseDash;
			spriteCrown.enabled = true;
		}

		public void Stop()
		{
			rigidbody.useGravity = false;
			doAction = DoActionStop;
		}

		public void SetModeVoid()
        {
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

			if (currentState == PlayerState.MOUSE) 
			{ 
				MouseElapsedTime += Time.deltaTime; 
			}
		}
		#endregion doAction

		#endregion State Machine

		IEnumerator AnimateScale(Vector3 origin, Vector3 target)
		{
			float journey = 0f;

			while (journey <= rescaleDuration)
			{
				journey = journey + Time.deltaTime;
				float percent = Mathf.Clamp01(journey / rescaleDuration);
				float curvePercent = rescaleAnimation.Evaluate(percent);

				transform.localScale = Vector3.LerpUnclamped(origin, target, curvePercent);

				yield return null;
			}
		}

		private IEnumerator DashCooldown()
		{
			canDash = false;

			yield return new WaitForSeconds(playerSpecs.DashCooldown);

			canDash = true;
		}
		
		public IEnumerator StunAfterCollision()
        {
			SetModeVoid();
			PlayParticleSlow();
			yield return new WaitForSeconds(secondStunAfterCollision);
			SetModeCat();
			Resume();
			StopParticleSlow();
			stunCoroutine = null;
		}

		public void OnMove(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();
		public void OnDash(InputAction.CallbackContext ctx)
		{
			if (canDash)
			{
				rigidbody.AddForce(velocity.normalized * currentDash, ForceMode.Impulse);

				StartCoroutine(DashCooldown());

				fx_Dash.Play();

				audioSource.clip = sounds_dash[UnityEngine.Random.Range(0, sounds_dash.Length)];
				audioSource.Play();
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
