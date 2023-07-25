using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class CharacterBrain : MonoBehaviour
{
    [Header("Character Operation Mode")] [SerializeField]
    private ProjectEnums.CharacterAction _characterAction;

    private PlayerController _characterController;
    
    // Start is called before the first frame update
    void Start()
    {
        _characterController = FindObjectOfType<PlayerController>();
        if(_characterController == null)
            Debug.LogError("Essential Component Missing", this);
        
        //Bruh wtf
        if(_characterAction != ProjectEnums.CharacterAction.MoveBetweenPoints) return;
        ExecuteCharacterMoveBetweenPoints();
    }

    public void ExecuteCharacterJumpStationary()
    {
        if(_characterAction != ProjectEnums.CharacterAction.JumpStationary) return;
        _characterController.StartIdle();
    }
    
    
    public void ExecuteCharacterMoveBetweenPoints()
    {
        if(_characterAction != ProjectEnums.CharacterAction.MoveBetweenPoints) return;
        _characterController.MoveBetweenPositions();
    }
}
