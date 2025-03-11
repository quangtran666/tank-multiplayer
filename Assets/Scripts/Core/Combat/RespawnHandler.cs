using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private TankPlayer playerPrefab;
    [SerializeField] private float keptCoinsPercentage = 50;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        var players = FindObjectsByType<TankPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var player in players)
        {
            HandlePlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;

        TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(TankPlayer player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(TankPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(TankPlayer player)
    {
        var keptCoin = player.CoinsWallet.TotalCoins.Value * (keptCoinsPercentage / 100f);
        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId, (int)keptCoin));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientID, int keptCoin)
    {
        yield return null;

        var playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientID);
        playerInstance.CoinsWallet.TotalCoins.Value += keptCoin;
    }
}
