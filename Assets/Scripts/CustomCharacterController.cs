using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class CustomCharacterController : MonoBehaviour
{

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
    public UnityEvent<bool> onAcceleration;
    public UnityEvent<bool, bool> onDeceleration;
    public UnityEvent onStoppedMove;
    public UnityEvent<bool> onFlipBoost;

    //This is confusing, needs to be fixed
    [Header("Select Variable Parameter")] 
    [SerializeField] private ProjectEnums.JumpParameters jumpParameters;
    
    [Header("Jump Parameter")] 
    [Range(0, 10)]
    [SerializeField] private float gravityScale = 1f;
    [Range(0f, 20f)]
    [SerializeField] private float jumpHeight = 10f;
    [Range(0.1f, 10f)]
    [SerializeField] private float timeToReachJumpPeak = 1f;
    
    
    
    [Header("OptionalSettings")] 
    [SerializeField] private bool jumpAnimations = false;
    [SerializeField] private bool jumpVfx = false;
    [SerializeField] private GameObject jumpVfxParent;
    [Space(5)]
    [SerializeField] private bool altFallGravity = false;
    [Range(0, 10)]
    [SerializeField] private float altFallGravityScale = 1.2f;
    

    [Header("Jump Events")]
    public UnityEvent<float> onJumpApexReached;
    public UnityEvent<int> onJumpInitiated;
    public UnityEvent<int> onLanded;

    private Rigidbody2D _rb;
    private Vector2 _localGravity;
    private Vector2 _localAltFallGravity;
    private float _localGravityY;
    private float _jumpVelocity;
    private float _previousFrameYVelocity;
    private float _pointA;
    private float _pointB;
    private float _moveDirection;
    private float _moveSpeed;
    private float _accelerationRate;
    private float _decelerationRate;
    private bool _isGrounded;
    private bool _isFirstCollision = true;
    private bool _hasStopped = true;
    private bool _hasCompletedPause = true;
    private bool _flip;

    public ProjectEnums.MovementState MovementEventFlag { get; private set; }

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

    }

    // Start is called before the first frame update
    private void Start()
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
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
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
                jumpHeight = (_localGravityY * Mathf.Pow(timeToReachJumpPeak, 2f)) / 2f;
            }
                break;
            case ProjectEnums.JumpParameters.Gravity:
            {
                _localGravityY = 2 * jumpHeight / Mathf.Pow(timeToReachJumpPeak, 2f);
            }
                break;
            case ProjectEnums.JumpParameters.Time:
            {
                timeToReachJumpPeak = Mathf.Sqrt((2 * jumpHeight)/_localGravityY);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FixedUpdate()
    {
        if (altFallGravity)
        {
            if (_rb.velocity.y < 0.01f)
            {
                //Apply Alt Gravity
                _rb.AddForce(_localAltFallGravity, ForceMode2D.Force);
                return;
            }   
        }
        
        //ApplyGravity
        _rb.AddForce(_localGravity, ForceMode2D.Force);
    }

    private void Update()
    {
        if (_isGrounded) return;

        if (Mathf.Sign(_rb.velocity.y) < 0f && _previousFrameYVelocity >= 0f)
        {
            onJumpApexReached?.Invoke(0);
        }

        _previousFrameYVelocity = Mathf.Sign(_rb.velocity.y);
    }

    public void Jump()
    {
        
        if (!_isGrounded) return;
        _isGrounded = false;

        onJumpInitiated?.Invoke(0);
        
        //reset y-velocity for consistency
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
                
        //formula to reach height <jumpHeight>
        //under gravity <maxGravityAcceleration> 
        // v0=sqrt(2gY)
        _jumpVelocity = Mathf.Sqrt(2f * _localGravityY * jumpHeight);
        _rb.velocity = new Vector2(_rb.velocity.x, _jumpVelocity);

        _previousFrameYVelocity = 1f;
    }

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

    private void FlipBoost(bool isMovingRight)
    {
        if (!flipBoost) return;

        _moveSpeed = maxMovementSpeed * flipBoostScale;
        onFlipBoost?.Invoke(isMovingRight);

    }

    public bool Decelerate()
    {
        //deceleration
        if (!(_moveSpeed > 0f))
        {
            onStoppedMove?.Invoke();
            MovementEventFlag = ProjectEnums.MovementState.Stopped;
            return true;
        }

        if (MovementEventFlag != ProjectEnums.MovementState.Decelerating)
        {
            onDeceleration?.Invoke(!(_rb.velocity.x > 0f), _isGrounded);
            MovementEventFlag = ProjectEnums.MovementState.Decelerating;
        }

        _moveSpeed -= _decelerationRate * Time.fixedDeltaTime;
        _moveSpeed = Mathf.Clamp(_moveSpeed, 0f, maxMovementSpeed);
        
        var dir = _rb.velocity.x > 0f ? 1f : -1f;
        _rb.velocity = new Vector2(_moveSpeed * dir, _rb.velocity.y);
        
        return false;
    }
    
    public void Accelerate(bool isInputRight)
    {
        //acceleration
        if (!(_moveSpeed < maxMovementSpeed)) return;
        
        if (MovementEventFlag != ProjectEnums.MovementState.Accelerating)
        {
            MovementEventFlag = ProjectEnums.MovementState.Accelerating;
            onAcceleration?.Invoke(isInputRight);
        }
        _moveSpeed += _accelerationRate * Time.fixedDeltaTime;
        _moveSpeed = Mathf.Clamp(_moveSpeed, 0f, maxMovementSpeed);

        var dir = isInputRight ? 1f : -1f;
        _rb.velocity = new Vector2(_moveSpeed * dir, _rb.velocity.y);
    }
    
    private IEnumerator IdleThenMove()
    {
        _hasCompletedPause = false;
        //yield return new WaitForSeconds(Random.Range(0f, 2f));
        yield return new WaitForSeconds(1.2f);
        FlipBoost(_moveDirection>0);
        _hasCompletedPause = true;
    }

}
