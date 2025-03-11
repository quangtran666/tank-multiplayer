using System;
using Unity.Netcode;
using UnityEngine;

public class CoinsWallet : NetworkBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin bountyCoinPrefab;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 50f;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;

    public NetworkVariable<int> TotalCoins = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        coinRadius = bountyCoinPrefab.GetComponent<CircleCollider2D>().radius;
        health.OnDie += HandlePlayerDie;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        health.OnDie -= HandlePlayerDie;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<Coin>(out var coin)) return;

        int cointValue = coin.Collect();

        if (!IsServer) return;

        TotalCoins.Value += cointValue;
    }

    private void HandlePlayerDie(Health health)
    {
        var bountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100f));
        var bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue < minBountyCoinValue) return;

        for (int i = 0; i < bountyCoinCount; i++)
        {
            var bountyCoinInstance = Instantiate(bountyCoinPrefab, GetSpawnPoint(), Quaternion.identity);
            bountyCoinInstance.SetCoinValue(bountyCoinValue);
            bountyCoinInstance.NetworkObject.Spawn();
        }
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            var spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }
}
