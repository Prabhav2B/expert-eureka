using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(CustomCharacterController))]
public class PlayerController : MonoBehaviour
{
    private Vector2 _receivedInput;
    private Rigidbody2D _rb;

    private CustomCharacterController _characterController;

    private void Start()
    {
        _characterController = GetComponent<CustomCharacterController>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public void MovementInputReceived(InputAction.CallbackContext context)
    {
        //if(!context.performed) return;
        
        _receivedInput = context.ReadValue<Vector2>();

    }
    
    public void JumpInputReceived(InputAction.CallbackContext context)
    {
        if(!context.performed) return;
        
        _characterController.Jump();
    }

    protected void FixedUpdate()
    {

        // if input provided        
        if (!Mathf.Approximately(_receivedInput.x, 0f))
        {
            var isInputRight = _receivedInput.x > 0f;
            var rbMovingRight = _rb.velocity.x > 0f;
            
            // if character is already accelerating or decelerating
            // AND
            // if rigidbody direction is different from input direction
            if (_characterController.MovementEventFlag != ProjectEnums.MovementState.Stopped 
                && rbMovingRight != isInputRight)
            {
                _characterController.Decelerate();
                return;
            }

            _characterController.Accelerate(isInputRight);
        }
        else
        {
            if(_characterController.MovementEventFlag == ProjectEnums.MovementState.Stopped)
                return;

            _characterController.Decelerate();
        }

    }
}
