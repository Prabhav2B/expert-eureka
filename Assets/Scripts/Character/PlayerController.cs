using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CustomCharacterController
{
    private Vector2 _receivedInput;
    private bool _isOverloaded;
    private bool _isRooted;
    
    public void MovementInputReceived(InputAction.CallbackContext context)
    {
        if (_isOverloaded || _isRooted)
        {
            _receivedInput = Vector2.zero;
            return;
        }
        
        _receivedInput = context.ReadValue<Vector2>();

    }

    public void JumpInputReceived(InputAction.CallbackContext context)
    {
        if (_isOverloaded || _isRooted) return;
        
        if(context.started)
            Jump();
        
        if(context.canceled)
            JumpCancelled();
    }

    protected new void Update()
    {
        base.Update();
        //Debug.Log("Overloaded: " + _isOverloaded + "   ----   " + "Rooted: " +  _isRooted);
    }

    protected new void FixedUpdate()
    {
        base.FixedUpdate();

        // if input provided        
        if (!Mathf.Approximately(_receivedInput.x, 0f))
        {
            var isInputRight = _receivedInput.x > 0f;
            var rbMovingRight = Rb.velocity.x > 0f;
            
            // if character is already accelerating or decelerating
            // AND
            // if rigidbody direction is different from input direction
            if (MovementEventFlag != ProjectEnums.MovementState.Stopped 
                && rbMovingRight != isInputRight)
            {
                Decelerate();
                return;
            }

            Accelerate(isInputRight);
        }
        else
        {
            if(MovementEventFlag == ProjectEnums.MovementState.Stopped)
                return;

            Decelerate();
        }

    }

    public void Recharging(float chargeAmount)
    {
        _isOverloaded = false;
        //TODO CHANGE THIS
        airControl = true;
    }

    public void Overload()
    {
        _isOverloaded = true;
        airControl = false;
    }
    
    public void RootBegin()
    {
        Debug.Log("Rooted");
        _isRooted = true;
    }
    
    public void RootEnd()
    {
        _isRooted = false;
    }
}
