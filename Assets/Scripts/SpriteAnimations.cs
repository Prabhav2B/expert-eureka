using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SpriteAnimations : MonoBehaviour
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

    private Sequence _mySequence;

    private void Start()
    {
        _originalScale = transform.localScale;
        _mySequence = DOTween.Sequence();
    }

    public void SquashAndStretch(int id)
    {
        _mySequence.Kill();
        _mySequence = DOTween.Sequence();
        if (_selfDisabled) return;
        _mySequence.Append(transform.DOScale(squashScale, squashTime).OnComplete(StretchAndReset));
        
    }
    
    public void SquashAndReset(int id)
    {
        _mySequence.Kill();
        _mySequence = DOTween.Sequence();
        if (_selfDisabled) return;
        _mySequence.Append(transform.DOScale(squashScale, squashTime).OnComplete(Reset));
        
    }

    public void StretchAndReset()
    {
        _mySequence.Kill();
        _mySequence = DOTween.Sequence();
        if(_selfDisabled) return;
        _mySequence.Append(transform.DOScale(stretchScale, stretchTime).OnComplete(Reset));
        

    }
    
    public void StretchAndReset(int id)
    {
        _mySequence.Kill();
        _mySequence = DOTween.Sequence();
        if(_selfDisabled) return;
        _mySequence.Append(transform.DOScale(stretchScale, stretchTime).OnComplete(Reset));
        
    }
    
    public void Stretch(int id)
    {
        _mySequence.Kill();
        _mySequence = DOTween.Sequence();
        if(_selfDisabled) return;
        _mySequence.Append(transform.DOScale(new Vector3(0.8f, 1.2f, 1f), stretchTime*4f));
            
    }

    public void Reset(int id)
    {
        _mySequence.Kill();
        _mySequence = DOTween.Sequence();
        if(_selfDisabled) return;
        _mySequence.Append(transform.DOScale(_originalScale, resetTime));
        
    }
    
    
    private void Reset()
    {
        _mySequence.Kill();
        _mySequence = DOTween.Sequence();
        if(_selfDisabled) return;
        _mySequence.Append(transform.DOScale(_originalScale, resetTime));
        
    }

    public void DisableSelf()
    {
        _selfDisabled = true;
    }
}
