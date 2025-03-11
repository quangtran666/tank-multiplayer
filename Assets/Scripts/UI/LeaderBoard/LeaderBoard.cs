using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoard : NetworkBehaviour
{
    [SerializeField] private Transform leaderBoardContentHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderBoardEntityPrefab;
    [SerializeField] private int maxLeaderboardEntities = 8;

    private NetworkList<LeaderBoardEntityState> leaderboardEntites;
    private readonly List<LeaderBoardEntityDisplay> leaderboardEntitiesList = new();

    private void Awake()
    {
        leaderboardEntites = new();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderboardEntites.OnListChanged += HandleLeaderboardListEntitiesChanged;
            foreach (var entity in leaderboardEntites)
            {
                HandleLeaderboardListEntitiesChanged(new NetworkListEvent<LeaderBoardEntityState>
                {
                    Type = NetworkListEvent<LeaderBoardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (IsServer)
        {
            var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                HandlePlayerSpawned(player);
            }

            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderboardEntites.OnListChanged -= HandleLeaderboardListEntitiesChanged;
        }

        if (IsServer)
        {
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }

    private void HandlePlayerSpawned(TankPlayer tankPlayer)
    {
        leaderboardEntites.Add(new LeaderBoardEntityState
        {
            ClientID = tankPlayer.OwnerClientId,
            PlayerName = tankPlayer.PlayerName.Value,
            Coins = 0
        });

        tankPlayer.CoinsWallet.TotalCoins.OnValueChanged += (oldCoin, newCoins) => HandleCoinsChanged(tankPlayer.OwnerClientId, newCoins);
    }

    private void HandlePlayerDespawned(TankPlayer tankPlayer)
    {
        if (leaderboardEntites == null) return;

        foreach (var entity in leaderboardEntites)
        {
            if (entity.ClientID == tankPlayer.OwnerClientId)
            {
                leaderboardEntites.Remove(entity);
                break;
            }
        }

        tankPlayer.CoinsWallet.TotalCoins.OnValueChanged -= (oldCoin, newCoins) => HandleCoinsChanged(tankPlayer.OwnerClientId, newCoins);
    }

    private void HandleLeaderboardListEntitiesChanged(NetworkListEvent<LeaderBoardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Add:
                if (leaderboardEntitiesList.Any(x => x.ClientID == changeEvent.Value.ClientID)) return;
                var leaderboardEntityDisplay = Instantiate(leaderBoardEntityPrefab, leaderBoardContentHolder);
                leaderboardEntityDisplay.Initialise(changeEvent.Value.ClientID, changeEvent.Value.PlayerName, changeEvent.Value.Coins);
                leaderboardEntitiesList.Add(leaderboardEntityDisplay);
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Remove:
                var entityToRemove = leaderboardEntitiesList.FirstOrDefault(x => x.ClientID == changeEvent.Value.ClientID);
                if (entityToRemove != null)
                {
                    entityToRemove.transform.SetParent(null);
                    Destroy(entityToRemove.gameObject);
                    leaderboardEntitiesList.Remove(entityToRemove);
                }
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Value:
                var entityToUpdate = leaderboardEntitiesList.FirstOrDefault(x => x.ClientID == changeEvent.Value.ClientID);
                if (entityToUpdate != null)
                {
                    entityToUpdate.UpdateCoins(changeEvent.Value.Coins);
                }
                break;
        }

        leaderboardEntitiesList.Sort((x, y) => y.Coins.CompareTo(x.Coins));

        for (var i = 0; i < leaderboardEntitiesList.Count; i++)
        {
            leaderboardEntitiesList[i].transform.SetSiblingIndex(i);
            leaderboardEntitiesList[i].UpdateDisplayText();
            var shouldShow = i < maxLeaderboardEntities;
            leaderboardEntitiesList[i].gameObject.SetActive(shouldShow);
        }

        var myDisplay = leaderboardEntitiesList.FirstOrDefault(x => x.ClientID == NetworkManager.Singleton.LocalClientId);

        if (myDisplay != null)
        {
            if (myDisplay.transform.GetSiblingIndex() >= maxLeaderboardEntities)
            {
                leaderBoardContentHolder.GetChild(maxLeaderboardEntities - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }
    }

    private void HandleCoinsChanged(ulong clientId, int newCoins)
    {
        for (var i = 0; i < leaderboardEntites.Count; i++)
        {
            if (leaderboardEntites[i].ClientID == clientId)
            {
                leaderboardEntites[i] = new LeaderBoardEntityState
                {
                    ClientID = clientId,
                    PlayerName = leaderboardEntites[i].PlayerName,
                    Coins = newCoins
                };
                return;
            }
        }
    }
}
