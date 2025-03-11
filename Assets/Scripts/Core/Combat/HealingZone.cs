using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [SerializeField] private Image healPowerBar;
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCooldown = 60f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinsPerTick = 10;
    [SerializeField] private int healPerTick = 10;

    private float remainingCooldown;
    private float tickTimer;

    private List<TankPlayer> playersInZone = new();

    private NetworkVariable<int> HealPower = new();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, HealPower.Value);
        }

        if (IsServer)
        {
            HealPower.Value = maxHealPower;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPowerChanged;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (remainingCooldown > 0f)
        {
            remainingCooldown -= Time.deltaTime;

            if (remainingCooldown <= 0f)
            {
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }

        tickTimer += Time.deltaTime;
        if (tickTimer >= 1 / healTickRate)
        {
            foreach (var player in playersInZone)
            {
                if (HealPower.Value <= 0) break;

                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) continue;

                if (player.CoinsWallet.TotalCoins.Value < coinsPerTick) continue;

                // player.CoinsWallet.spent 
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (!collision.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;

        playersInZone.Add(player);
    }

    void OTriggerExit2D(Collider2D collision)
    {
        if (!IsServer) return;

        if (!collision.attachedRigidbody.TryGetComponent<TankPlayer>(out var player)) return;

        playersInZone.Remove(player);
    }

    private void HandleHealPowerChanged(int oldHealPower, int newHealPower)
    {
        healPowerBar.fillAmount = newHealPower / (float)maxHealPower;
    }
}
