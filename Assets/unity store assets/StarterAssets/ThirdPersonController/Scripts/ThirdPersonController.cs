using Unity.VisualScripting;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    interface IInteractable
    {
        public void Interact();
    }

    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        public static ThirdPersonController instance;

        [Header("Player")]
        [Tooltip("Player Health")]
        [SerializeField] private float PlayerHealth = 20.0f;
        float FullHealth;
        [Tooltip("Defense")]
        [SerializeField] private float Defense = 10.0f;
        [Space(10)]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Inventory control")]

        public GameObject InventoryUI;
        public GameObject FishingRod;
        public GameObject Sword;

        public Transform CastPoint;
        public GameObject castBauble;
        public float CastForce = 10.0f;
        private bool ReadyToCast = true;
        private Coroutine BobberReturnRoutine;

        private int CurrentHeldItem = 0;
        private GameObject currentBobber = null;

        public Transform interactSource;
        public float InteractRange = 250.0f;

        public float cooldownTime = 2.0f;
        [SerializeField] private float ReturnTime = 20.0f;
        public bool onCooldown = false;

        private int layerMask;

        private bool returning = false;

        private Transform SpawnPoint;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private int _animIDCast;
        private int _animIDFishingIdle;
        private int _animIDSwordIdle;
        private int _animIDSwordSwing;
        private int _animIDDeath;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private Slider healthbar;
        private TextMeshProUGUI armorRating;
        private Image HeldItem;

        public Sprite rodSprite;
        public Sprite swordSprite;

        private bool isAlive = true;

        public AudioClip[] swordequip;
        public AudioClip[] fishingRodEQ;
        public AudioClip[] painGrunt;
        public AudioClip[] deathgrunt;
        public AudioClip Castflip;
        public AudioClip swordswing;
        public AudioClip[] reelit;
        bool activeClip;

        public AudioClip BagUnzip;
        IEnumerator Cooldown()
        {
            onCooldown = true;
            yield return new WaitForSeconds(cooldownTime);
            ReadyToCast = true;
            onCooldown = false;
        }

        IEnumerator BobberReturn()
        {
            returning = true;
            //Debug.Log("return coroutine started");
            yield return new WaitForSeconds(ReturnTime);
            Debug.Log("return coroutine Executed");
            returning = false;
            Destroy(currentBobber);
            currentBobber = null;
            StartCoroutine(Cooldown());
        }

        IEnumerator DelayedCast(float delay)
        {
            yield return new WaitForSeconds(delay);
            currentBobber = Instantiate(castBauble, CastPoint.position, Quaternion.identity);
            Rigidbody rb = currentBobber.GetComponent<Rigidbody>();

            Ray raycast = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            if (Physics.Raycast(raycast, out RaycastHit hit, 1000f, layerMask))
            {
                Debug.DrawLine(_mainCamera.transform.position, hit.point, Color.red, 25f);
                Vector3 forceDir = (hit.point - CastPoint.position).normalized;

                Debug.Log(forceDir);
                Vector3 AppliedForce = forceDir * CastForce + transform.up * 5f;
                rb.AddForce(AppliedForce, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(transform.forward * CastForce + transform.up * 5f, ForceMode.Impulse);
            }
            _input.useItem = false;

            yield return new WaitForSeconds(2);
            Debug.Log("anim delay ended");
            _animator.SetBool(_animIDCast, false);

        }

        IEnumerator DelayAD(float delay)
        {
            yield return new WaitForSeconds(delay); 
            transform.position = SpawnPoint.position;
            isAlive = true;
            PlayerHealth = FullHealth;
            healthbar.value = PlayerHealth;

            _animator.SetBool(_animIDDeath, false);
        }

        IEnumerator AudioDelay(AudioClip clip)
        {
            yield return new WaitForSeconds(clip.length);
            activeClip = false;
        }
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            layerMask = ~LayerMask.GetMask("Player");
            instance = this;

            SpawnPoint = GameObject.Find("SpawnPoint").transform;
            FullHealth = PlayerHealth;

            healthbar = GameObject.Find("HealthBar").GetComponent<Slider>();
            healthbar.maxValue = FullHealth;
            healthbar.value = PlayerHealth;

            armorRating = GameObject.Find("Armor Rating").GetComponentInChildren<TextMeshProUGUI>();
            armorRating.text = "Defense: " + Defense.ToString();

            HeldItem = GameObject.Find("EquiptItem").GetComponent<Image>();
            HeldItem.sprite = null;


        }

        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _animator.SetLayerWeight(1, 0f);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            if (isAlive)
            {
                JumpAndGravity();
                GroundedCheck();
                Move();
                Interact();
                OpenInventory();
                UseItem();
                closeApp();
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDCast = Animator.StringToHash("Cast");
            _animIDFishingIdle = Animator.StringToHash("RodEquipt");
            _animIDSwordIdle = Animator.StringToHash("SwordEquipt");
            _animIDSwordSwing = Animator.StringToHash("SwordAttack");
            _animIDDeath = Animator.StringToHash("isDead");
    }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    //Debug.Log("jump detected");
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        private void Interact()
        {
            if (_input.interact)
            {
                Ray raycast = new Ray(interactSource.position, interactSource.forward);
                if (Physics.Raycast(raycast, out RaycastHit hitInfo, InteractRange))
                {
                    if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable obj))
                    {
                        obj.Interact();
                    }
                }

                _input.interact = false;
            }
        }

        private void OpenInventory()
        {
            if (_input.Inventory)
            {
                AudioSource.PlayClipAtPoint(BagUnzip,_controller.center);
                InventoryUI.SetActive(!InventoryUI.activeSelf);
                Cursor.visible = InventoryUI.activeSelf;
                if (InventoryUI.activeSelf == true)
                {
                    InventoryManager.Instance.listItems();
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    InventoryManager.Instance.ClearList();
                    Cursor.lockState = CursorLockMode.Locked;
                }
                _input.Inventory = false;
            }
        }

        public void EquipItem(int itemType)
        {

            CurrentHeldItem = itemType;
            switch (itemType)
            {
                case 0:
                    Debug.Log("No usable Item");
                    break;
                case 1:
                    //Debug.Log("Fishing Rod Equipt");
                    if (Sword.activeSelf == true)
                    {
                        Sword.SetActive(false);
                        _animator.SetBool(_animIDSwordIdle, false);
                    }
                    if(fishingRodEQ.Length > 0)
                    {
                        int index = Random.Range(0, fishingRodEQ.Length);
                        AudioSource.PlayClipAtPoint(fishingRodEQ[index], _controller.center);
                    }
                    FishingRod.SetActive(!FishingRod.activeSelf);
                    _animator.SetBool(_animIDFishingIdle, FishingRod.activeSelf);
                    if (FishingRod.activeSelf)
                    {
                        HeldItem.sprite = rodSprite;
                        _animator.SetLayerWeight(1, 1f);
                    }
                    else
                    {
                        _animator.SetLayerWeight(1, 0f);
                        HeldItem.sprite = null;
                    }
                    break;
                case 2:
                    if (FishingRod.activeSelf == true)
                    {
                        FishingRod.SetActive(false);
                        _animator.SetBool(_animIDFishingIdle, false);
                    }
                    if (swordequip.Length > 0)
                    {
                        int index = Random.Range(0, swordequip.Length);
                        AudioSource.PlayClipAtPoint(swordequip[index], _controller.center);
                    }
                    
                    Sword.SetActive(!Sword.activeSelf);
                    _animator.SetBool(_animIDSwordIdle, Sword.activeSelf);
                    if (Sword.activeSelf)
                    {
                        HeldItem.sprite = swordSprite;
                        _animator.SetLayerWeight(1, 1f);
                    }
                    else
                    {
                        HeldItem.sprite = null;
                        _animator.SetLayerWeight(1, 0f);
                    }
                    break;
                default:
                    Debug.Log("No usable Item");
                    break;
            }
        }

        private void UseItem()
        {
            if (_input.useItem)
            {
                switch (CurrentHeldItem)
                {
                    case 0:
                        Debug.Log("No usable Item");
                        break;
                    case 1:
                        //Debug.Log("Fishing Rod in use");
                        if (ReadyToCast)
                        {

                            CastBobber();
                        }
                        else
                        {
                            reelIn();
                        }
                        break;
                    case 2:
                        WeaponController.instance.attacking = _animator.GetBool(_animIDSwordSwing);
                        if (!WeaponController.instance.attacking)
                        {
                            AudioSource.PlayClipAtPoint(swordswing, _controller.center, 1f);
                            WeaponController.instance.Attack(_animator, _animIDSwordSwing);
                        }
                        break;
                    default:
                        Debug.Log("No usable Item");
                        break;
                }

            }
        }

        public void CastBobber()
        {
            ReadyToCast = false;
            _animator.SetBool(_animIDCast, true);
            AudioSource.PlayClipAtPoint(Castflip, _controller.center,1f);
            StartCoroutine(DelayedCast(2.0f));
        }

        private void reelIn()
        {
            if (currentBobber != null)
            {
                if (!returning) { BobberReturnRoutine = StartCoroutine(BobberReturn()); }
                if (!activeClip)
                {
                    activeClip = true;
                    if (reelit.Length > 0)
                    {
                        int index = Random.Range(0, reelit.Length);
                        AudioSource.PlayClipAtPoint(reelit[index], _controller.center, 1);
                        StartCoroutine(AudioDelay(Castflip));
                    }
                    
                }
                Vector3 ReelLoc = (interactSource.position - currentBobber.transform.position).normalized;
                float reelSpeed = Mathf.Lerp(1f,3f,Vector3.Distance(currentBobber.transform.position, interactSource.position));
                Rigidbody rb = currentBobber.GetComponent<Rigidbody>();
                FishingBobble FishingScript = currentBobber.GetComponent<FishingBobble>();

                Vector3 AppliedForce = ReelLoc * reelSpeed;
                rb.AddForce(AppliedForce, ForceMode.Impulse);

                if (FishingScript.FishingSpot != null && FishingScript.FishingSpot.catchDetected)
                {
                    FishingScript.FishingSpot.CatchFish();
                }

                if (Vector3.Distance(currentBobber.transform.position, interactSource.position) < 1f)
                {


                    if (BobberReturnRoutine != null)
                    {
                        //Debug.Log("Routine stopped");
                        StopCoroutine(BobberReturnRoutine);
                        BobberReturnRoutine = null;
                        returning = false;
                    }
                    //Debug.Log("bobber destroyed");
                    if (FishingScript != null && FishingScript.CaughtNCPS.Count > 0)
                    {

                        //Debug.Log("NPC CAUGHT");
                        foreach (NPCBehavior npc in FishingScript.CaughtNCPS)
                        {
                            EventManager.Instance.cstmevents.FishCollected();
                            npc.pickupItem();
                        }
                    }
                    else if (FishingScript != null && FishingScript.FishingSpot != null)
                    {
                        if(FishingScript.FishingSpot.caughtFish != null)
                        {
                            //Debug.Log("adding: " + FishingScript.FishingSpot.caughtFish + " to inventory");
                            EventManager.Instance.cstmevents.FishCollected();
                            InventoryManager.Instance.AddItem(FishingScript.FishingSpot.caughtFish); ;
                            FishingScript.FishingSpot.caughtFish = null;
                        }   
                    }
                    Destroy(currentBobber);
                    currentBobber = null;
                    StartCoroutine(Cooldown());
                }
            }
        }

        public void PlayerDamaged(float DMGVal)
        {
            PlayerHealth -= DMGVal / Defense;
            healthbar.value = PlayerHealth;
            if(painGrunt.Length > 0)
            {
                int index = Random.Range(0, painGrunt.Length);
                AudioSource.PlayClipAtPoint(painGrunt[index], _controller.center);
            }
            
            if (PlayerHealth <= 0)
            {
                isAlive = false;
                _animator.SetBool(_animIDDeath, true);
                if (deathgrunt.Length > 0)
                {
                    int index = Random.Range(0, deathgrunt.Length);
                    AudioSource.PlayClipAtPoint(deathgrunt[index], _controller.center, 1.8f);
                }
                if (InventoryManager.Instance.HasQuestItem(Items.ItemType.CombatQuest))
                {
                    foreach (Items item in InventoryManager.Instance.items)
                    {
                        if (item.itemType == Items.ItemType.CombatQuest)
                        {
                            InventoryManager.Instance.RemoveItem(item);
                        }
                    }
                }
                StartCoroutine(DelayAD(8));

            }
        }

        public void closeApp()
        {
            
            if (_input.close) { Debug.Log("ClosingApp");  Application.Quit(); }
        }
            
    }
}