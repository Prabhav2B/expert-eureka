using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GroundCheck : MonoBehaviour
{
    
    public LayerMask layerMask;
    public UnityEvent<int> layerEnterCollision;
    public UnityEvent<int> layerExitCollision;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        //returns an id for the ground type 
        //so that different sound effect of
        //particles can be triggered
        if ((layerMask.value & (1 << other.transform.gameObject.layer)) <= 0) return;
        layerEnterCollision?.Invoke(0);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //returns an id for the ground type 
        //so that different sound effect of
        //particles can be triggered
        if ((layerMask.value & (1 << other.transform.gameObject.layer)) <= 0) return;
        layerExitCollision?.Invoke(0);
    }
}
