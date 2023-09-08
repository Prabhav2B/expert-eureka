using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementVfx : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustStream;
    
    [SerializeField] private ParticleSystem dustExplosionLeft;
    [SerializeField] private ParticleSystem dustExplosionRight;
    
    private Rigidbody2D _rb;
    private CustomCharacterController _customCharacterController;
    
    private void Start()
    {
        _rb = GetComponentInParent<Rigidbody2D>();
        _customCharacterController = GetComponentInParent<CustomCharacterController>();
        
        if (_rb == null)
            Debug.LogWarning("RigidBody missing from parent", this);
        
        if (dustStream == null)
            Debug.LogWarning("Particle System not assigned", this);
        
        if (_customCharacterController == null)
            Debug.LogWarning("CustomCharacterController missing from parent", this);
    }
    
    public void StopMovementFx()
    {
        StopMovementFx(false, false);
    }
    
    public void StopMovementFx(bool isMovingRight, bool isGrounded)
    {
        dustStream.Stop();
    }
    
    public void StartMovementVfx()
    {
        if (gameObject.activeSelf == false) return;
        if(Mathf.Abs (_rb.velocity.x) <= _customCharacterController.MaxMovementSpeed - 1f) return;

        dustStream.Play();
        var dustStreamShape = dustStream.shape;
        var direction = _rb.velocity.x>0 ? 1 : -1;
        dustStreamShape.rotation = new Vector3(0, 0, 90f * direction);
    }
    public void StartMovementVfx(bool isMovingRight, bool isGrounded)
    {
        if (gameObject.activeSelf == false) return;
        if(!isGrounded) return;
        if(Mathf.Abs (_rb.velocity.x) <= _customCharacterController.MaxMovementSpeed - 1f) return;

        dustStream.Play();
        var dustStreamShape = dustStream.shape;
        var direction = isMovingRight ? 1 : -1;
        dustStreamShape.rotation = new Vector3(0, 0, 90f * direction);
    }
    
    public void TurnDustVfx(bool isMovingRight, bool isGrounded)
    {
        if(gameObject.activeSelf == false)return;
        if(!isGrounded) return;
        if(!Mathf.Approximately(Mathf.Abs (_rb.velocity.x)
               , _customCharacterController.MaxMovementSpeed)) return;

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
