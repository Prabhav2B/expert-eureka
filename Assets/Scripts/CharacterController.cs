using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D _rb;
    private bool _grounded;

    // Start is called before the first frame update
    void Start()
    {
        _grounded = true;
        _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void JumpPhysics()
    {
        
        if (!_grounded) return;
        _grounded = false;

        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        
        var jumpVec = Vector2.up * 10f;
        _rb.AddForce(jumpVec, ForceMode2D.Impulse);
    }

    void JumpBasic()
    {
    }

    public void PlayerHitGround(int id)
    {
        
        _grounded = true;
        JumpPhysics();
    }
}
