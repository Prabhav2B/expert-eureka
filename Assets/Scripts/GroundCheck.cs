using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class GroundCheck : MonoBehaviour
{
    
    public LayerMask layerMask;
    public UnityEvent<int> onLayerEnterCollision;
    public UnityEvent<int> onLayerExitCollision;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        //returns an id for the ground type 
        //so that different sound effect of
        //particles can be triggered
        if ((layerMask.value & (1 << other.transform.gameObject.layer)) <= 0) return;
        onLayerEnterCollision?.Invoke(0);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //returns an id for the ground type 
        //so that different sound effect of
        //particles can be triggered
        if ((layerMask.value & (1 << other.transform.gameObject.layer)) <= 0) return;
        onLayerExitCollision?.Invoke(0);
    }
}
