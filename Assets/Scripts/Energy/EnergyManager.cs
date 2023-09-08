using System;
using UnityEngine;
using UnityEngine.Events;

public class EnergyManager : MonoBehaviour
{

    [SerializeField] private float totalEnergy;
    [SerializeField] private int energyCellsCount;

    private SpriteRenderer[] _energySprites;

    public float SingleCellEnergy { get; private set; }

    public UnityEvent onEnergyDepleted;
    public UnityEvent<float> onEnergyConsumed;
    
    public UnityEvent onEnergyRecharged;
    public UnityEvent<float> onEnergyRecovery;

    public UnityEvent onRootBegin;
    public UnityEvent onRootEnd;

    private void Awake()
    {
        SingleCellEnergy = totalEnergy / energyCellsCount;
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
        totalEnergy -= depletionAmount;
    }

    private void Update()
    {
        Debug.Log(totalEnergy);
    }
}
