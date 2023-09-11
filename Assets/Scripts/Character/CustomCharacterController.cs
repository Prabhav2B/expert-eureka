using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public class CustomCharacterController : MonoBehaviour
{

    [SerializeField] private EnergyConfig energyConfig;
    
    [Header("Walk Parameter")] 
    [Range(0.1f, 20f)]
    [SerializeField] private float maxMovementSpeed = 5f;
    [Range(0.1f, 20f)]
    [SerializeField] private float timeToReachMaxSpeed = 2f;
    [Range(0.1f, 20f)]
    [SerializeField] private float timeToStop = 2f;
    
    [Header("OptionalSettings")] 
    [SerializeField] private bool moveAnimations = false;
    [SerializeField] private bool moveVfx = false;
    [SerializeField] private GameObject moveVfxParent;
    [SerializeField] private bool flipBoost = false;
    [Range(0f, 1f)]
    [SerializeField] private float flipBoostScale = 0.33f;

    [Header("Move Events")] 
    public UnityEvent<bool, bool> onAcceleration;
    public UnityEvent<bool, bool> onDeceleration;
    public UnityEvent<bool, bool> onMaxSpeed;
    public UnityEvent onStoppedMove;
    public UnityEvent<bool, bool> onFlipBoost;

    //This is confusing, needs to be fixed
    [Header("Select Variable Parameter")] 
    [SerializeField] private ProjectEnums.JumpParameters jumpParameters;
    
    [Header("Jump Parameter")] 
    [Range(0, 10)]
    [SerializeField] private float gravityScale = 1f;
    [Range(0f, 20f)]
    [SerializeField] private float maxJumpHeight = 10f;
    [Range(0.1f, 10f)]
    [SerializeField] private float timeToReachMaxJumpPeak = 1f;

    [Header("OptionalSettings")] 
    [SerializeField] private bool jumpAnimations = false;
    [SerializeField] private bool jumpVfx = false;
    [SerializeField] private GameObject jumpVfxParent;
    [Space(5)]
    [SerializeField] private bool altFallGravity = false;
    [Range(0, 10)]
    [SerializeField] private float altFallGravityScale = 1.2f;

    [SerializeField] protected bool airControl = true;
    [Range(0f, 1f)] [SerializeField] private float airControlAmount = 1f;

    [Header("Jump Events")]
    public UnityEvent<float> onJumpApexReached;
    public UnityEvent<int> onJumpInitiated;
    public UnityEvent<int> onLanded;


    protected Rigidbody2D Rb;
    private Vector2 _localGravity;
    private Vector2 _localAltFallGravity;
    private float _localGravityY;
    private float _velocityForMaxJumpHeight;
    private float _previousFrameYVelocity;
    private float _pointA;
    private float _pointB;
    private float _moveDirection;
    private float _moveSpeed;
    private float _prevMoveSpeed;
    private float _accelerationRate;
    private float _decelerationRate;
    private float _energyConsumed;
    private float _energyThreshold;
    private bool _isJumpCancelled;
    private bool _isGrounded;
    private bool _isFirstCollision = true;
    private bool _hasStopped = true;
    private bool _hasCompletedPause = true;
    private bool _flip;
    private bool _isEnergyCharacter;
    private EnergyManager _energyManager;
    

    public ProjectEnums.MovementState MovementEventFlag { get; private set; }
    public float MaxMovementSpeed => maxMovementSpeed;
    
    private const float BaseGravity = 9.8f;

    private void Awake()
    {
        if (!jumpAnimations)
        {
            var animComponent = GetComponentInChildren<SpriteAnimations>();
            if (animComponent != null)
            {
                animComponent.DisableJumpAnimation();
            }
        }
        
        if (!moveAnimations)
        {
            var animComponent = GetComponentInChildren<SpriteAnimations>();
            if (animComponent != null)
            {
                animComponent.DisableTiltAnimation();
            }
        }
        
        Rb = GetComponent<Rigidbody2D>();
        Rb.gravityScale = 0f;

        _energyManager = GetComponentInChildren<EnergyManager>();
        if (!(_energyManager == null))
            _isEnergyCharacter = true;
    }

    // Start is called before the first frame update
    protected void Start()
    {
        // Set isFirstFrame to true on Start to allow events to be fired after the first frame
        _isFirstCollision = true;

        if (jumpVfxParent == null)
        {
            Debug.LogWarning("Move Vfx parent not assigned", this);
        }
        else
        {
            jumpVfxParent.SetActive(jumpVfx);
        }

        _isGrounded = true;
        _localGravityY = gravityScale * BaseGravity;

        //this function can alter : jumpHeight, timeToReachJumpPeak and _localGravityY
        JumpParameterCalculation();
        
        _localGravity = new Vector2(0f, -_localGravityY);
        _localAltFallGravity = new Vector2(0f, -altFallGravityScale * BaseGravity);

        _pointA = transform.position.x + 5f;
        _pointB = transform.position.x - 5f;
        _moveDirection = 1f;
        _accelerationRate = maxMovementSpeed / timeToReachMaxSpeed;
        _decelerationRate = maxMovementSpeed / timeToStop;

        if (moveVfxParent == null)
        {
            Debug.LogWarning("Move Vfx parent not assigned", this);
        }
        else
        {
            moveVfxParent.SetActive(moveVfx);
        }

        MovementEventFlag = ProjectEnums.MovementState.Stopped;
    }
    
    private void JumpParameterCalculation()
    {
        switch (jumpParameters)
        {
            case ProjectEnums.JumpParameters.Height:
            {
                maxJumpHeight = (_localGravityY * Mathf.Pow(timeToReachMaxJumpPeak, 2f)) / 2f;
            }
                break;
            case ProjectEnums.JumpParameters.Gravity:
            {
                _localGravityY = 2 * maxJumpHeight / Mathf.Pow(timeToReachMaxJumpPeak, 2f);
            }
                break;
            case ProjectEnums.JumpParameters.Time:
            {
                timeToReachMaxJumpPeak = Mathf.Sqrt((2 * maxJumpHeight)/_localGravityY);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected void FixedUpdate()
    {
        if (altFallGravity)
        {
            if (Rb.velocity.y < 0.01f)
            {
                //Apply Alt Gravity
                Rb.AddForce(_localAltFallGravity, ForceMode2D.Force);
                return;
            }   
        }
        
        if (_isJumpCancelled)
        {
            if (Rb.velocity.y >= _velocityForMaxJumpHeight / 3f)
                Rb.velocity += new Vector2( 0, (-1f * _velocityForMaxJumpHeight) / 4f);

            _isJumpCancelled = false;
        }
        
        //ApplyGravity
        Rb.AddForce(_localGravity, ForceMode2D.Force);
    }

    protected void Update()
    {
        if (!_isGrounded)
        {
            if (Mathf.Sign(Rb.velocity.y) < 0f && _previousFrameYVelocity >= 0f)
            {
                onJumpApexReached?.Invoke(0);
            }

            _previousFrameYVelocity = Mathf.Sign(Rb.velocity.y);
        }

        //Handle energy related events for jumping
        //Movement energy in Accelerate function
        if (!_isEnergyCharacter) return;

        if (!_isJumpCancelled && Rb.velocity.y > 0.1f)
            _energyManager.onEnergyConsumed?.Invoke(energyConfig.EnergyConsumedPerJumpTick * Time.deltaTime);


        if (MovementEventFlag != ProjectEnums.MovementState.Accelerating) return;
        //Handle energy related events for movement
        //Jump energy in Fixed Update
        _energyManager.onEnergyConsumed?.Invoke(energyConfig.EnergyConsumedPerMoveTick * Time.deltaTime);

    }

    public void Jump()
    {
        
        if (!_isGrounded) return;

        _isGrounded = false;

        onJumpInitiated?.Invoke(0);
        
        //reset y-velocity for consistency
        Rb.velocity = new Vector2(Rb.velocity.x, 0f);
                
        //formula to reach height <jumpHeight>
        //under gravity <maxGravityAcceleration> 
        // v0=sqrt(2gY)
        _velocityForMaxJumpHeight = Mathf.Sqrt(2f * _localGravityY * maxJumpHeight);
        Rb.velocity = new Vector2(Rb.velocity.x, _velocityForMaxJumpHeight);
        
    }

    public void CharacterLeftGround()
    {
        _isGrounded = false;
    }

    public void JumpCancelled()
    {
        
        if (_isGrounded) return;
        if(Rb.velocity.y <= 0f) return;
        
        
        //reset y-velocity for consistency
        //_rb.velocity = new Vector2(_rb.velocity.x, 0f);

        _isJumpCancelled = true;
    }
    
    public void PlayerHitGround(int id)
    {
        // Check if it's the first frame, and if so, skip invoking the event
        if (_isFirstCollision)
        {
            _isFirstCollision = false;
            _isGrounded = true;
            return;
        }
        _isGrounded = true;
        onLanded?.Invoke(0);
        
        if(airControl) return;
        Rb.velocity = new Vector2(Rb.velocity.x/2f, Rb.velocity.y);
    }

    private void FlipBoost(bool isMovingRight)
    {
        if (!flipBoost) return;

        _moveSpeed = maxMovementSpeed * flipBoostScale;
        onFlipBoost?.Invoke(isMovingRight, _isGrounded);

    }

    public bool Decelerate()
    {
        
        if(!_isGrounded && !airControl) return false;
        
        var effectiveDeceleration = !_isGrounded ? _decelerationRate * airControlAmount : _decelerationRate;

        
        //deceleration
        if (!(_moveSpeed > 0f))
        {
            onStoppedMove?.Invoke();
            MovementEventFlag = ProjectEnums.MovementState.Stopped;
            return true;
        }

        if (MovementEventFlag != ProjectEnums.MovementState.Decelerating)
        {
            onDeceleration?.Invoke(!(Rb.velocity.x > 0f), _isGrounded);
            MovementEventFlag = ProjectEnums.MovementState.Decelerating;
        }

        _moveSpeed -= effectiveDeceleration * Time.fixedDeltaTime;
        _moveSpeed = Mathf.Clamp(_moveSpeed, 0f, maxMovementSpeed);
        
        var dir = Rb.velocity.x > 0f ? 1f : -1f;
        Rb.velocity = new Vector2(_moveSpeed * dir, Rb.velocity.y);
        return false;
    }
    
    public void Accelerate(bool isInputRight)
    {
        
        if(!_isGrounded && !airControl) return;

        var effectiveAcceleration = !_isGrounded ? _accelerationRate * airControlAmount : _accelerationRate;

        //if at max speed, end here
        if (!(Mathf.Abs(Rb.velocity.x) < maxMovementSpeed)) return;

        _moveSpeed = Mathf.Abs(Rb.velocity.x);
        
        if (MovementEventFlag != ProjectEnums.MovementState.Accelerating)
        {
            MovementEventFlag = ProjectEnums.MovementState.Accelerating;
            onAcceleration?.Invoke(isInputRight, _isGrounded);
        }
        
        _moveSpeed += effectiveAcceleration * Time.fixedDeltaTime;
        _moveSpeed = Mathf.Clamp(_moveSpeed, 0f, maxMovementSpeed);

        if ( !Mathf.Approximately(_prevMoveSpeed, maxMovementSpeed) 
             && Mathf.Approximately(_moveSpeed, maxMovementSpeed))
        {
            onMaxSpeed?.Invoke(isInputRight, _isGrounded);
        }

        _prevMoveSpeed = _moveSpeed;
        
        var dir = isInputRight ? 1f : -1f;
        Rb.velocity = new Vector2(_moveSpeed * dir, Rb.velocity.y);
        
        
    }
    
    [Serializable]
    public struct EnergyConfig {
        [SerializeField] private float energyConsumedPerJumpTick;
        [SerializeField] private float energyConsumedPerMoveTick;
 
        public float EnergyConsumedPerJumpTick => energyConsumedPerJumpTick;
        public float EnergyConsumedPerMoveTick => energyConsumedPerMoveTick;
    }
    
    //Might refactor these
    #region Brain-Related-Functions
    public void MoveBetweenPositions(Vector3 pointA = default, Vector3 pointB = default)
    {
        if (pointA != default && pointB != default)
        {
            if (pointA.x >= pointB.x)
            {
                _pointA = pointA.x;
                _pointB = pointB.x;
            }
            else
            {
                _pointA = pointB.x;
                _pointB = pointA.x;
            }
        }

        _moveSpeed = 0f;
        StartCoroutine(MoveBetween());
    }
    public void StartIdle()
    {
        if(!_isGrounded) return;
        StartCoroutine(IdleThenJump());
    }
    private IEnumerator IdleThenJump()
    {
        //yield return new WaitForSeconds(Random.Range(0f, 2f));
        yield return new WaitForSeconds(1.2f);
        Jump();
    }
    private IEnumerator MoveBetween()
    {
        while (true)
        {
            if (transform.position.x > _pointA && transform.position.x > _pointB && !_flip)
            {
                _hasStopped = Decelerate();
                if (_hasStopped)
                {
                    _moveDirection = -1f;
                    _flip = true;
                    StartCoroutine(IdleThenMove());
                }
            }

            if (transform.position.x < _pointA && transform.position.x < _pointB && _flip)
            {
                _hasStopped = Decelerate();
                if (_hasStopped)
                {
                    _moveDirection = 1f;
                    _flip = false;
                    StartCoroutine(IdleThenMove());
                }
            }

            if (_hasStopped && _hasCompletedPause)
            {
                Accelerate(_moveDirection>0);
            }

            yield return new WaitForFixedUpdate();
        }
    }
    private IEnumerator IdleThenMove()
    {
        _hasCompletedPause = false;
        //yield return new WaitForSeconds(Random.Range(0f, 2f));
        yield return new WaitForSeconds(1.2f);
        FlipBoost(_moveDirection>0);
        _hasCompletedPause = true;
    }

    #endregion
    
    
    
}
