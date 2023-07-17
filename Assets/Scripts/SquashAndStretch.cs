using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SquashAndStretch : MonoBehaviour
{
    [Header("Time Parameters")]
    [Range(0f, 2f)]
    [SerializeField] private float squashTime = 0.1f;
    [Range(0f, 2f)]
    [SerializeField] private float stretchTime = 0.1f;
    [Range(0f, 2f)]
    [SerializeField] private float resetTime = 0.1f;


    [Header("Scale Values")] 
    [SerializeField] private Vector3 squashScale = new Vector3(1.4f, 0.6f, 1f);
    [SerializeField] private Vector3 stretchScale = new Vector3(0.6f, 1.4f, 1f);

    private bool _selfDisabled;
    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void Squash(int id)
    {
        if(!_selfDisabled)
            transform.DOScale(squashScale, squashTime).OnComplete(Stretch);
    }

    public void Stretch()
    {
        if(!_selfDisabled)
            transform.DOScale(stretchScale, stretchTime).OnComplete(Reset);
    }
    
    public void Stretch(int id)
    {
        if(!_selfDisabled)
            transform.DOScale(stretchScale, stretchTime).OnComplete(Reset);
    }

    private void Reset()
    {
        transform.DOScale(_originalScale, resetTime);
    }

    public void DisableSelf()
    {
        _selfDisabled = true;
    }
}
