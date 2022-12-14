///-----------------------------------------------------------------
/// Author : Jean Juillard
/// Date : 27/09/2021 19:08
///-----------------------------------------------------------------

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using nn.hid;


namespace Com.IsartDigital.ChaseTag.ChaseTag
{
    public delegate void PlayerEventHandler(Player player);

    [RequireComponent(typeof(Rigidbody))]
    public class Player : MonoBehaviour
    {
        public static event PlayerEventHandler OnCollectibleCollected;
        //public static event PlayerEventHandler OnMouseCaught;

        [Header("States")] [SerializeField] private PlayerSpecs playerSpecs = default;

        private PlayerState currentState = PlayerState.NORMAL;
        public PlayerState CurrentState => currentState;

        [HideInInspector] public float currentSpeed;

        private Vector3 movementInput;

        private Rigidbody rb;
        private Vector3 velocity = Vector3.zero;

        // [Header("Inputs")]
        // [SerializeField] private string horizontalInput = "Horizontal";
        // [SerializeField] private string verticalInput = "Vertical";
        // [SerializeField] private KeyCode dashInput = default;
        [Header("Sound")] [SerializeField] private AudioSource audioSource = default;
        [SerializeField] private AudioClip[] sounds_dash = default;
        [SerializeField] private AudioClip[] sounds_collision = default;

        [Header("Collisions")] [SerializeField]
        private string collectibleTag = "Collectible";

        [SerializeField] private string playerTag = "Player";
        [SerializeField] private string wallTag = "Wall";
        [SerializeField] private string borderTag = "Border";
        [SerializeField, Range(0.1f, 4f)] private float secondStunAfterCollision = default;
        [SerializeField] private float stunDrag = 100000;

        [SerializeField] private Vector3 respawnPosition;

        [Header("Particles")] [SerializeField] private ParticleSystem fx_Slow = default;
        [SerializeField] private ParticleSystem fx_Dash = default;
        [SerializeField] private ParticleSystem fx_Explosion = default;
        [SerializeField] private ParticleSystem fx_Shield = default;
        [SerializeField] private ParticleSystem fx_Crown = default;
        [SerializeField] private TrailRenderer fx_CrownTrail = default;

        [SerializeField] private Renderer playerRenderer = default;
        [SerializeField] private SpriteRenderer spriteCrown = default;

        [SerializeField] private Vector3 sizeNormal;
        [SerializeField] private Vector3 sizeCat;
        [SerializeField] private Vector3 sizeMouse;

        [SerializeField] private float rescaleDuration = 1f;
        [SerializeField] private AnimationCurve rescaleAnimation;

        [SerializeField] private TrailRenderer trail = default;
        [SerializeField] private AnimationCurve[] trailWidth3Mode = default;

        [SerializeField] private TextAsset dashVibrationAsset = default;
        [SerializeField] private TextAsset collisionVibrationAsset = default;


        private CameraShake cameraShake = default;

        private Coroutine stunCoroutine;
        public int npadID = 0;

        private float SpeedMultiplicator = 1;

        //private int numCollectiblesCollected = 0;
        //public int NumCollectiblesCollected => numCollectiblesCollected;

        public bool haveCrown = false;

        private float currentDash;
        private bool canDash = true;

        public float prevSqrMagnitude;

        private Action doAction;

        public Coroutine trapCoroutine;
        public Trap trap;

        public bool isTrapped = false;

        public float MouseElapsedTime { get; private set; } = 0f;


        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            doAction = DoActionStop;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            SetModeVoid();
            // Resume();
            audioSource.clip = sounds_collision[1];
            audioSource.Play();

            cameraShake = Camera.main.GetComponentInParent<CameraShake>();
        }

        private void Update()
        {
            doAction();
            KeepCrownInPlace();

            //int id = PlayerManager.Instance.GetPlayerId(this);
        }

        void FixedUpdate()
        {
            prevSqrMagnitude = rb.velocity.sqrMagnitude;
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

            var listPlayer = PlayerManager.Instance.playerInfos;

            for (int i = 0; i < listPlayer.Length; i++)
            {
                if (listPlayer[i].player = this)
                {
                    transform.position = listPlayer[i].spawnPosition;
                    trail.Clear();
                    StartCoroutine(dontMove());
                    return;
                }
            }
        }

        IEnumerator dontMove()
        {
            SpeedMultiplicator = 0;

            yield return new WaitForSeconds(1);

            SpeedMultiplicator = 1;
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
            //rb.useGravity = false;
            //rb.isKinematic = true;
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

                audioSource.clip = sounds_collision[1];
                audioSource.Play();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag(playerTag) || collision.collider.CompareTag(wallTag) ||
                collision.collider.CompareTag(borderTag))
            {
                cameraShake.enabled = true;
                audioSource.clip = sounds_collision[0];
                audioSource.Play();
                StartCoroutine(InputManager.Instance.PlayVibrationFile(collisionVibrationAsset, npadID));
            }

            if (currentState == PlayerState.CAT && collision.collider.CompareTag(playerTag) && stunCoroutine == null)
            {
                //OnMouseCaught?.Invoke(this);

                Player playerCollided = collision.gameObject.GetComponent<Player>();

                // Change state only if the other player is mouse
                if (playerCollided.currentState == PlayerState.MOUSE)
                {
                    Debug.Log("j'ai eu la souris !");

                    playerCollided.ChangeCrown();
                    ChangeCrown();

                    fx_Crown.Play();
                    fx_CrownTrail.enabled = true;

                    playerCollided.fx_Crown.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                    playerCollided.fx_CrownTrail.enabled = false;

                    ParticleSystem.MainModule main = fx_Shield.main;
                    main.duration = secondStunAfterCollision;

                    fx_Shield.Play();

                    audioSource.clip = sounds_collision[1];
                    audioSource.Play();
                }
            }
        }

        #region State Machine

        public void SetModeNormal()
        {
            currentState = PlayerState.NORMAL;

            currentSpeed = playerSpecs.NormalSpeed;
            rb.drag = playerSpecs.NormalDrag;
            currentDash = playerSpecs.NormalDash;
            spriteCrown.enabled = false;

            trail.widthCurve = trailWidth3Mode[0];
            fx_Crown.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void SetModeCat()
        {
            currentState = PlayerState.CAT;

            if (trap)
                trap.CancelTrap(this);

            currentSpeed = playerSpecs.CatSpeed;
            rb.drag = playerSpecs.CatDrag;
            currentDash = playerSpecs.CatDash;
            spriteCrown.enabled = false;

            trail.widthCurve = trailWidth3Mode[1];
        }

        public void SetModeMouse()
        {
            currentState = PlayerState.MOUSE;

            if (trap)
                trap.CancelTrap(this);

            currentSpeed = playerSpecs.MouseSpeed;
            rb.drag = playerSpecs.MouseDrag;
            currentDash = playerSpecs.MouseDash;
            spriteCrown.enabled = true;

            trail.widthCurve = trailWidth3Mode[2];
        }

        public void Stop()
        {
            if (!rb) rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            doAction = DoActionStop;
        }

        public void SetModeVoid()
        {
            doAction = DoActionStop;
        }

        public void Resume()
        {
            Debug.Log("RESUME");
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

            rb.useGravity = true;
            doAction = DoActionNormal;
        }

        #region doAction

        private void DoActionStop()
        {
        }

        private void DoActionNormal()
        {
            velocity.x = movementInput.x * SpeedMultiplicator;
            velocity.z = movementInput.y * SpeedMultiplicator;

            velocity = velocity.normalized * (currentSpeed * Time.deltaTime);

            rb.AddForce(velocity, ForceMode.VelocityChange);

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
            InputManager.Instance.SetVibration(0f, 0f, 0f, 0f, PlayerManager.Instance.GetPlayerId(this));
        }

        public void PlayCoroutineStun()
        {
            StartCoroutine(Stun());
        }

        public IEnumerator Stun()
        {
            float originalAngularDrag = rb.angularDrag;

            rb.angularDrag = stunDrag;

            Debug.Log("Stuned : " + rb.angularDrag);

            yield return new WaitForSeconds(secondStunAfterCollision);

            rb.angularDrag = originalAngularDrag;

            Debug.Log("UNStuned : " + rb.angularDrag);
        }

        public IEnumerator StunAfterCollision()
        {
            /*float originalAngularDrag = rb.angularDrag;

            SetModeVoid();
            rb.angularDrag = stunDrag;*/
            PlayParticleSlow();

            yield return new WaitForSeconds(secondStunAfterCollision);

            /*rb.angularDrag = originalAngularDrag;*/
            SetModeCat();
            Resume();
            StopParticleSlow();
            stunCoroutine = null;
        }

        public void OnMove(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();

        public void OnDash(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;

            Debug.Log("LOG ========================== DASH ===================");

            if (canDash && isActiveAndEnabled)
            {
                if (!rb) rb = GetComponent<Rigidbody>();
                rb.AddForce(velocity.normalized * currentDash, ForceMode.Impulse);

                StartCoroutine(DashCooldown());
                StartCoroutine(InputManager.Instance.PlayVibrationFile(dashVibrationAsset, npadID));

                fx_Dash.Play();

                audioSource.clip = sounds_dash[UnityEngine.Random.Range(0, sounds_dash.Length)];
                audioSource.Play();
            }
        }

        public void OnJoin(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;

            Debug.Log("LOG ========================== JOIN ===================");
            if (PlayerManager.Instance.AllPlayersReady())
            {
                UIManager.Instance.BtnPlay();
            }
            else
            {
                UIManager.Instance.IsReady(PlayerManager.Instance.GetPlayerId(this), true);
            }
        }
        
        public void OnSettings(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;

            Debug.Log("LOG ========================== SETTINGS ===================");

            UIManager.Instance.DisplaySetting(ctx);
        }

        public void OnDisplayQuit(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;

            Debug.Log("LOG ========================== DISPLAY QUIT ===================");

            UIManager.Instance.DisplayQuit(ctx);
        }
        
        public void OnQuit(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;

            Debug.Log("LOG ========================== QUIT ===================");

            UIManager.Instance.Quit(ctx);
        }
        
        public void OnNextRound(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;

            Debug.Log("LOG ========================== NEXT ROUND ===================");
            UIManager.Instance.NextRound(ctx);
        }

        public void OnHelp(InputAction.CallbackContext ctx)
        {
            if (ctx.phase != InputActionPhase.Performed) return;

            Debug.Log("LOG ========================== HELP ===================");

            UIManager.Instance.DisplayHelp(ctx);
        }

        public void ResetPlayer()
        {
            SetModeNormal();
            SetSize(PlayerState.NORMAL);
            haveCrown = false;
            fx_Crown.Stop();
            fx_CrownTrail.enabled = false;
            fx_Shield.Stop();
            fx_Slow.Stop();
            cameraShake.enabled = false;
            MouseElapsedTime = 0;
            trail.Clear();
        }
    }


    public enum PlayerState
    {
        NORMAL,
        CAT,
        MOUSE
    }
}