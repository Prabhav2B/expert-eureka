using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{

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
    public UnityEvent<float> omJumpApexReached;
    public UnityEvent<int> onJumpInitiated;
    public UnityEvent<int> onLanded;

    private Rigidbody2D _rb;
    private Vector2 _localGravity;
    private Vector2 _localAltFallGravity;
    private float _localGravityY;
    private bool _grounded;
    private float _jumpVelocity;

    private const float BaseGravity = 9.8f;

    private void Awake()
    {
        if(squashAndStretch)
            return;
        
        var squashAndStretchComp = GetComponentInChildren<SquashAndStretch>();
        if (squashAndStretchComp != null)
        {
            squashAndStretchComp.DisableSelf();
        }
            
    }

    // Start is called before the first frame update
    void Start()
    {
        _grounded = true;
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _localGravityY = gravityScale * BaseGravity;

        //this function can alter : jumpHeight, timeToReachJumpPeak and _localGravityY
        JumpParameterCalculation();
        
        _localGravity = new Vector2(0f, -_localGravityY);
        _localAltFallGravity = new Vector2(0f, -altFallGravityScale * BaseGravity);

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
    }

    public void PlayerHitGround(int id)
    {
        _grounded = true;
        onLanded?.Invoke(0);
        Jump();
    }
    
    [Obsolete("Not used any more, using a velocity based implementation", true)]
    private void JumpPhysics()
    {
        
        if (!_grounded) return;
        _grounded = false;
        
        //reset y-velocity for consistency
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);

        var jumpVec = Vector2.up * 10f;
        _rb.AddForce(jumpVec, ForceMode2D.Impulse);
    }
}
