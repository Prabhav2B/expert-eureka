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

    [Header("Tilt Parameters")] 
    
    [Range(0f, 2f)]
    [SerializeField] private float tiltTime = 0.1f;
    [SerializeField] private float tiltAmount = 10f;


    private bool _jumpAnimationDisabled;
    private bool _tiltAnimationDisabled;
    private Vector3 _originalScale;
    private Vector3 _originalRot;

    private Sequence _squashStretchSequence;
    private Sequence _tiltSequence;

    private void Start()
    {
        _originalScale = transform.localScale;
        _originalRot = transform.localRotation.eulerAngles;
            
        _squashStretchSequence = DOTween.Sequence();
        _tiltSequence = DOTween.Sequence();
    }

    public void SquashAndStretch(int id)
    {
        if(_jumpAnimationDisabled) return;
        _squashStretchSequence.Kill();
        _squashStretchSequence = DOTween.Sequence();
        _squashStretchSequence.Append(transform.DOScale(squashScale, squashTime).OnComplete(StretchAndReset));
        
    }
    
    public void SquashAndReset(int id)
    {
        if(_jumpAnimationDisabled) return;
        _squashStretchSequence.Kill();
        _squashStretchSequence = DOTween.Sequence();
        _squashStretchSequence.Append(transform.DOScale(squashScale, squashTime).OnComplete(ResetScale));
        
    }

    public void StretchAndReset()
    {
        if(_jumpAnimationDisabled) return;
        _squashStretchSequence.Kill();
        _squashStretchSequence = DOTween.Sequence();
        _squashStretchSequence.Append(transform.DOScale(stretchScale, stretchTime).OnComplete(ResetScale));
        

    }
    
    public void StretchAndReset(int id)
    {
        if(_jumpAnimationDisabled) return;
        _squashStretchSequence.Kill();
        _squashStretchSequence = DOTween.Sequence();
        _squashStretchSequence.Append(transform.DOScale(stretchScale, stretchTime).OnComplete(ResetScale));
        
    }
    
    public void Stretch(int id)
    {
        if(_jumpAnimationDisabled) return;
        _squashStretchSequence.Kill();
        _squashStretchSequence = DOTween.Sequence();
        _squashStretchSequence.Append(transform.DOScale(new Vector3(0.8f, 1.2f, 1f), stretchTime*4f));
            
    }

    public void ResetScale(int id)
    {
        if(_jumpAnimationDisabled) return;
        _squashStretchSequence.Kill();
        _squashStretchSequence = DOTween.Sequence();
        _squashStretchSequence.Append(transform.DOScale(_originalScale, resetTime));
        
    }
    
    
    private void ResetScale()
    {
        if(_jumpAnimationDisabled) return;
        _squashStretchSequence.Kill();
        _squashStretchSequence = DOTween.Sequence();
        _squashStretchSequence.Append(transform.DOScale(_originalScale, resetTime));
        
    }

    public void Tilt(bool isMovingRight)
    {
        if (_tiltAnimationDisabled) return;
        _tiltSequence.Kill();
        _tiltSequence = DOTween.Sequence();

        var direction = isMovingRight ? -1 : 1;
        
        _tiltSequence.Append(transform.DOLocalRotate( new Vector3(0f, 0f, tiltAmount * direction), tiltTime));
    }

    public void ResetTilt()
    {
        if (_tiltAnimationDisabled) return;
        _tiltSequence.Kill();
        _tiltSequence = DOTween.Sequence();
        _tiltSequence.Append(transform.DOLocalRotate( _originalRot, tiltTime));
    }
    

    public void DisableJumpAnimation()
    {
        _jumpAnimationDisabled = true;
    }
    public void DisableTiltAnimation()
    {
        _tiltAnimationDisabled = true;
    }
}
