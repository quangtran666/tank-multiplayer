using System;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private int ownerPriority = 11;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinsWallet CoinsWallet { get; private set; }

    public NetworkVariable<FixedString32Bytes> PlayerName = new();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientID(OwnerClientId);
            PlayerName.Value = userData.UserName;
            OnPlayerSpawned?.Invoke(this);
        }

        if (!IsOwner) return;

        cinemachineCamera.Priority = ownerPriority;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }
}