using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{

    [Header("Jump Implementation")] [SerializeField]
    private ProjectEnums.JumpType jumpType;

    [Header("Physics Jump Parameter")] 
    [Range(0, 100)]
    [SerializeField] private float jumpForce;
    [Range(0, 100)]
    [SerializeField] private float gravity = 9.8f;
    
    [Header("Quasi Manual Jump Parameter")] 
    [Range(0, 100)]
    [SerializeField] private float jumpHeight;
    [Range(0, 100)]
    [SerializeField] private float maxGravityAcceleration = 9.8f;
    
    [Header("Jump Events")]
    public UnityEvent<float> omJumpApexReached;
    public UnityEvent<int> onJumpInitiated;
    public UnityEvent<int> onLanded;

    
    private Rigidbody2D _rb;
    private bool _grounded;

    // Start is called before the first frame update
    void Start()
    {
        _grounded = true;
        _rb = GetComponent<Rigidbody2D>();

        if(jumpType == ProjectEnums.JumpType.EnginePhysics)
            Physics2D.gravity = new Vector2(0, -gravity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void JumpPhysics()
    {
        
        if (!_grounded) return;
        _grounded = false;
        
        //reset y-velocity for consistency
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        
        var jumpVec = Vector2.up * jumpForce;
        _rb.AddForce(jumpVec, ForceMode2D.Impulse);
    }

    void JumpBasic()
    {
        
        if (!_grounded) return;
        _grounded = false;

        //reset y-velocity for consistency
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
                
        //formula to reach height <jumpHeight>
        //under gravity <maxGravityAcceleration> 
        // v0=sqrt(2gY)
        var jumpVelocity = Mathf.Sqrt(2f * maxGravityAcceleration * jumpHeight);
        _rb.velocity = new Vector2(_rb.velocity.x, jumpVelocity);
    }

    public void PlayerHitGround(int id)
    {
        _grounded = true;
        
        if(jumpType == ProjectEnums.JumpType.EnginePhysics)
            JumpPhysics();
        else
            JumpBasic();
    }
}
