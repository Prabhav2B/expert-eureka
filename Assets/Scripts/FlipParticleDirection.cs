using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlipParticleDirection : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustStream;
    
    [SerializeField] private ParticleSystem dustExplosionLeft;
    [SerializeField] private ParticleSystem dustExplosionRight;
    
    private Rigidbody2D _rb;
    
    private void Start()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        if (dustStream == null)
        {
            Debug.LogWarning("Particle System not assigned", this);
        }
    }

    public void StopMovementFx(bool isMovingRight, bool isGrounded)
    {
        if(!isGrounded) return;
        dustStream.Stop();
    }

    public void SetDirection(bool isMovingRight)
    {
        dustStream.Play();
        var dustStreamShape = dustStream.shape;
        var direction = isMovingRight ? 1 : -1;
        dustStreamShape.rotation = new Vector3(0, 0, 90f * direction);
    }
    
    public void TurnDustVfx(bool isMovingRight, bool isGrounded)
    {
        if(!isGrounded) return;
        if(Mathf.Abs(_rb.velocity.x) <  6f) return;
        if (isMovingRight)
        {
            dustExplosionLeft.Play();
        }
        else
        {
            dustExplosionRight.Play();
        }
    }
    
    
}
