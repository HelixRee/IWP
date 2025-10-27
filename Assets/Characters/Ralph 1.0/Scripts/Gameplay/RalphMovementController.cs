using StarterAssets;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */
[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class RalphMovementController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    //public AudioClip LandingAudioClip;
    //public AudioClip[] FootstepAudioClips;
    //[Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;
    [Tooltip("Gravity used when the character is grounded")]
    public float GroundedGravity = -0.2f;
    [Tooltip("Gravity multiplier at max jump height")]
    public float GravityJumpMultiplier = 0.8f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public Vector3 GroundCheckOffset = Vector3.zero;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    // player
    private float _speed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeedZ;
    private int _animIDSpeedX;
    private int _animIDGrounded;
    private int _animIDFallTrigger;
    private int _animIDJumpTrigger;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDLeftFootFlag;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    [Header("References")]
    public Animator Animator;
    public RalphProxyAnimator ProxyAnimator;
    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

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
    }
    private void OnValidate()
    {
        _controller = GetComponent<CharacterController>();
    }
    private void Start()
    {
        //Time.timeScale = 0.2f;

        if (!Animator)
            _hasAnimator = TryGetComponent(out Animator);
        else
            _hasAnimator = true;

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
    private Vector3 _groundNormal = Vector3.zero;
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Cancel velocity when hitting head
        if (hit.point.y > _controller.bounds.max.y && hit.moveLength < 0.1f && _verticalVelocity > 0)
            _verticalVelocity = 0f;
    }
    private void Update()
    {
        if (!Animator)
            _hasAnimator = TryGetComponent(out Animator);
        else
            _hasAnimator = true;

        JumpAndGravity();
        GroundedCheck();
        //Slide();
        Move();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeedZ = Animator.StringToHash("SpeedZ");
        _animIDSpeedX = Animator.StringToHash("SpeedX");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDLeftFootFlag = Animator.StringToHash("isOnLeftFoot");
        _animIDFallTrigger = Animator.StringToHash("fallTrigger");
        _animIDJumpTrigger = Animator.StringToHash("jumpTrigger");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 offset = Vector3.zero;
        if (_controller) offset = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * _controller.center;

        Vector3 spherePosition = new Vector3(transform.position.x + offset.x, transform.position.y,
            transform.position.z + offset.z);
        spherePosition += transform.localRotation * GroundCheckOffset;

        bool prevGrounded = Grounded;
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            Animator.SetBool(_animIDGrounded, Grounded);
            if (!Grounded && prevGrounded && !_input.willJump)
            {
                Animator.SetTrigger(_animIDFallTrigger);
                //Debug.Log("Falling");
            }

        }

        if (ProxyAnimator)
        {
            ProxyAnimator.IsGrounded = Grounded;
        }
    }
    private void Slide()
    {
        //if (!Grounded || _groundNormal.magnitude < 0.3f) return;
        //_controller.Move(_groundNormal * Time.deltaTime);
    }
    private void Move()
    {
        // set handTarget speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the handTarget speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to handTarget speed
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
        DriftPrevention();

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        // update animator if using character
        if (_hasAnimator)
        {
            Vector3 relativeVel = _controller.transform.InverseTransformDirection(_controller.velocity);
            Animator.SetFloat(_animIDSpeedX, Mathf.Lerp(Animator.GetFloat(_animIDSpeedX), relativeVel.x, 12f * Time.deltaTime));
            Animator.SetFloat(_animIDSpeedZ, Mathf.Lerp(Animator.GetFloat(_animIDSpeedZ), relativeVel.z, 12f * Time.deltaTime));
            //Animator.SetFloat(_animIDSpeedZ, 3);
            if (Animator.GetFloat(_animIDSpeedX) < 0.001f) Animator.SetFloat(_animIDSpeedX, 0f);
            if (Animator.GetFloat(_animIDSpeedZ) < 0.001f) Animator.SetFloat(_animIDSpeedZ, 0f);
        }
    }
    private void DriftPrevention()
    {
        if (_speed < 0.01f) _speed = 0f;
    }
    private bool _jumpActive = false;
    private bool _jumpTriggered = false;
    private void JumpAndGravity()
    {
        if (!Grounded)
            _input.willJump = false;
        if (_input.willJump)
        {
            if (!_jumpTriggered)
                Animator.SetTrigger(_animIDJumpTrigger);
            _jumpTriggered = true;

        }
        else
            _jumpTriggered = false;
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                //Animator.SetBool(_animIDJump, false);
                Animator.SetBool(_animIDFreeFall, false);
            }
            if (ProxyAnimator)
            {
                ProxyAnimator.IsFalling = false;
            }


            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity * GravityJumpMultiplier);

                //_input.willJump = false;

                // activate jumping flag
                _jumpActive = true;
            }

            // willJump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
            //if (!_input.jump)
            //    _input.jumpHeld = false;
        }
        else
        {
            // reset the willJump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else if (_verticalVelocity < 0f)
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    Animator.SetBool(_animIDFreeFall, true);
                }
                if (ProxyAnimator)
                {
                    ProxyAnimator.IsFalling = true;
                }
            }

            // if we are not grounded, do not willJump
            _input.jump = false;
            _input.willJump = false; // Coyote time here

            // if willJump is not held anymore cancel all future willJump inputs until grounded
            if (!_input.jumpHeld)
                _jumpActive = false;
            //if (!_jumpActive)
            //    _input.jumpHeld = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            float gravityMultiplier = _jumpActive && _verticalVelocity > 0f ? GravityJumpMultiplier : 1.0f;
            _verticalVelocity += Gravity * gravityMultiplier * Time.deltaTime;
        }

        // stop our velocity dropping infinitely when grounded
        if (_verticalVelocity < 0.0f && (Grounded || _controller.isGrounded))
        {
            _verticalVelocity = GroundedGravity;
        }
        //Debug.Log(_verticalVelocity);

        // update animator if using character
        if (_hasAnimator)
        {
            Animator.SetBool(_animIDJump, _input.willJump);
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

        Vector3 offset = Vector3.zero;
        if (_controller) offset = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * _controller.center;
        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x + offset.x, transform.position.y, transform.position.z + offset.z) + transform.localRotation * GroundCheckOffset,
            GroundedRadius);
    }
    public void OnStep(bool isLeft)
    {
        Animator.SetBool(_animIDLeftFootFlag, isLeft);
    }
    //private void OnFootstep(AnimationEvent animationEvent)
    //{
    //    if (animationEvent.animatorClipInfo.weight > 0.5f)
    //    {
    //        if (FootstepAudioClips.Length > 0)
    //        {
    //            var index = Random.Range(0, FootstepAudioClips.Length);
    //            AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
    //        }
    //    }
    //}

    //private void OnLand(AnimationEvent animationEvent)
    //{
    //    if (animationEvent.animatorClipInfo.weight > 0.5f)
    //    {
    //        AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
    //    }
    //}
}
