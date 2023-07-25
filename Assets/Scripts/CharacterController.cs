using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{

    [Header("Walk Parameter")] 
    [Range(0f, 20f)]
    [SerializeField] private float maxMovementSpeed = 5f;
    //[Range(0f, 20f)]
    //[SerializeField] private float acceleration = 10f;
    //[Range(0.1f, 10f)]
    //[SerializeField] private float timeToReachJumpPeak = 1f;
    
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
    [SerializeField] private bool squashAndStretch = false;
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
    private bool _grounded;
    private float _jumpVelocity;
    private float _previousFrameYVelocity;
    private bool _isFirstFrame = true;
    private float _pointA;
    private float _pointB;
    private float _moveDirection;
    
    private const float BaseGravity = 9.8f;

    private void Awake()
    {
        if(squashAndStretch)
            return;
        
        var squashAndStretchComp = GetComponentInChildren<SpriteAnimations>();
        if (squashAndStretchComp != null)
        {
            squashAndStretchComp.DisableSelf();
        }

    }

    // Start is called before the first frame update
    private void Start()
    {
        // Set isFirstFrame to true on Start to allow events to be fired after the first frame
        _isFirstFrame = true;
        
        _grounded = true;
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
        if (_grounded) return;

        if (Mathf.Sign(_rb.velocity.y) < 0f && _previousFrameYVelocity >= 0f)
        {
            onJumpApexReached?.Invoke(0);
        }

        _previousFrameYVelocity = Mathf.Sign(_rb.velocity.y);
    }

    private void Jump()
    {
        
        if (!_grounded) return;
        _grounded = false;

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

        StartCoroutine(MoveBetween());
    }

    public void PlayerHitGround(int id)
    {
        // Check if it's the first frame, and if so, skip invoking the event
        if (_isFirstFrame)
        {
            _isFirstFrame = false;
            _grounded = true;
            return;
        }
        _grounded = true;
        onLanded?.Invoke(0);
    }
    
    public void StartIdle()
    {
        if(!_grounded) return;
        StartCoroutine(Idleing());
    }

    private IEnumerator Idleing()
    {
        //yield return new WaitForSeconds(Random.Range(0f, 2f));
        yield return new WaitForSeconds(1.2f);
        Jump();
    }

    private IEnumerator MoveBetween()
    {
        while (true)
        {

            if (transform.position.x > _pointA && transform.position.x > _pointB)
                _moveDirection = -1f;
            if(transform.position.x < _pointA && transform.position.x < _pointB)
                _moveDirection = 1f;

            _rb.velocity = new Vector2(maxMovementSpeed * _moveDirection, 0f);
            yield return new WaitForFixedUpdate();    
        }
        
    }
}
