using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnergyManager : MonoBehaviour
{

    [SerializeField] private float maxEnergy;
    [SerializeField] private int energyCellsCount;

    private SpriteRenderer[] _energySprites;
    private bool _isDepleted;
    private float _currentEnergyAmount;

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

    private void OnEnable()
    {
        onEnergyConsumed.AddListener(DepleteEnergy);
    }
    
    private void OnDisable()
    {
        onEnergyConsumed.RemoveListener(DepleteEnergy);
    }

    private void DepleteEnergy(float depletionAmount)
    {
        if (_isDepleted) return;
        
        _currentEnergyAmount -= depletionAmount;
        _currentEnergyAmount = Mathf.Clamp(_currentEnergyAmount, 0f, maxEnergy);

        if (!(_currentEnergyAmount <= 0)) return;
        _isDepleted = true;
        onEnergyDepleted?.Invoke();
    }
    
}
