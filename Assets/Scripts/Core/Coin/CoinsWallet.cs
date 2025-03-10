using Unity.Netcode;
using UnityEngine;

public class CoinsWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new();

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Coin>(out var coin)) return;

        int cointValue = coin.Collect();

        if (!IsServer) return;

        TotalCoins.Value += cointValue;
    }
}
