using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CustomCharacterController))]
public class CharacterBrain : MonoBehaviour
{
    [Header("Character Operation Mode")] 
    [SerializeField] private ProjectEnums.CharacterAction characterAction;

    private CustomCharacterController _characterController;
    
    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CustomCharacterController>();
        if(_characterController == null)
            Debug.LogError("Essential Component Missing", this);
        
        //Bruh wtf
        if(characterAction != ProjectEnums.CharacterAction.MoveBetweenPoints) return;
        ExecuteCharacterMoveBetweenPoints();
    }

    public void ExecuteCharacterJumpStationary()
    {
        if(characterAction != ProjectEnums.CharacterAction.JumpStationary) return;
        _characterController.StartIdle();
    }
    
    
    public void ExecuteCharacterMoveBetweenPoints()
    {
        if(characterAction != ProjectEnums.CharacterAction.MoveBetweenPoints) return;
        _characterController.MoveBetweenPositions();
    }
}
