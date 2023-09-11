using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EnergyManager : MonoBehaviour
{

    [SerializeField] private float maxEnergy;
    [SerializeField] private int energyCellsCount;

    [SerializeField] private float chargeAmount;
    
    private SpriteRenderer[] _energySprites;
    private bool _isDepleted;
    private bool _isRecharging;
    private bool _isMaxCharged;
    private float _currentEnergyAmount;
    private Color _originalSpriteColor;

    public float SingleCellEnergy { get; private set; }

    public UnityEvent onEnergyDepleted;
    public UnityEvent<float> onEnergyConsumed;
    
    public UnityEvent onEnergyRecharged;
    public UnityEvent<float> onEnergyRecovery;

    public UnityEvent onRootBegin;
    public UnityEvent onRootEnd;

    private void Awake()
    {
        _currentEnergyAmount = maxEnergy;
        SingleCellEnergy = _currentEnergyAmount / energyCellsCount;
    }

    private void Start()
    {
        _energySprites = GetComponentsInChildren<SpriteRenderer>();
        _originalSpriteColor = _energySprites[0].color;
    }

    private void EvaluateEnergyUI()
    {
        var cellsAvailable = Mathf.CeilToInt(_currentEnergyAmount / SingleCellEnergy);

        //if(cellsAvailable == energyCellsCount) return;

        for (int i = 0; i < cellsAvailable; i++)
        {
            _energySprites[i].color = _originalSpriteColor;
        }

        for (int i = cellsAvailable ; i < energyCellsCount; i++)
        {
            _energySprites[i].color = Color.red;
        }

    }

    private void OnEnable()
    {
        onEnergyConsumed.AddListener(DepleteEnergy);
    }
    
    private void OnDisable()
    {
        onEnergyConsumed.RemoveListener(DepleteEnergy);
    }

    private void Update()
    {
        _isDepleted = Mathf.Approximately(_currentEnergyAmount, 0f);
        _isMaxCharged = Mathf.Approximately(_currentEnergyAmount, maxEnergy);
        
        //Debug.Log(_currentEnergyAmount);
        
        EvaluateEnergyUI();
        
        if (_isMaxCharged || !_isRecharging) return;
        RechargeEnergy();
    }

    private void DepleteEnergy(float depletionAmount)
    {
        if (_isDepleted) return;
        
        _currentEnergyAmount -= depletionAmount;
        _currentEnergyAmount = Mathf.Clamp(_currentEnergyAmount, 0f, maxEnergy);

        if (!Mathf.Approximately(_currentEnergyAmount, 0f)) return;
        onEnergyDepleted?.Invoke();
    }
    
    private void RechargeEnergy()
    {

        _currentEnergyAmount += chargeAmount * Time.deltaTime;
        _currentEnergyAmount = Mathf.Clamp(_currentEnergyAmount, 0f, maxEnergy);
        
        onEnergyRecovery?.Invoke(_currentEnergyAmount);

        //if (!Mathf.Approximately(_currentEnergyAmount, maxEnergy)) return;
        //onEnergyRecharged?.Invoke();
    }

    public void RootInputReceived(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onRootBegin?.Invoke();
            _isRecharging = true;
        }

        if (!context.canceled) return;
        _isRecharging = false;
        onRootEnd?.Invoke();
    }
    
}
