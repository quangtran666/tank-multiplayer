using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Image healthBarPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) return;
 
        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(0, health.CurrentHealth.Value); // Initialize the health bar
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) return;

        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int previousValue, int newValue)
    {
        healthBarPrefab.fillAmount = (float)newValue / health.MaxHealth;
    }
}
