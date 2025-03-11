using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable
{
    private NetworkManager networkManager;
    private Dictionary<ulong, string> ClientIdToAuthId = new();
    private Dictionary<string, UserData> AuthIDToUserData = new();

    public NetworkServer(NetworkManager networkManager)
    {
        this.networkManager = networkManager;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += OnNetworkReady;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        var userData = JsonUtility.FromJson<UserData>(payload);

        ClientIdToAuthId[request.ClientNetworkId] = userData.UserAuthID;
        AuthIDToUserData[userData.UserAuthID] = userData;

        response.Approved = true;
        response.Position = SpawnPoint.GetRandomSpawnPos();
        response.Rotation = Quaternion.identity;
        response.CreatePlayerObject = true;
    }

    private void OnNetworkReady()
    {
        networkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientID)
    {
        if (ClientIdToAuthId.TryGetValue(clientID, out var authId))
        {
            ClientIdToAuthId.Remove(clientID);
            AuthIDToUserData.Remove(authId);
        }
    }

    public UserData GetUserDataByClientID(ulong clientID)
    {
        if (ClientIdToAuthId.TryGetValue(clientID, out var authId) && AuthIDToUserData.TryGetValue(authId, out var userData))
        {
            return userData;
        }

        return null;
    }

    public void Dispose()
    {
        if (networkManager != null)
        {
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            networkManager.OnServerStarted -= OnNetworkReady;
            networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        if (networkManager.IsListening)
        {
            networkManager.Shutdown();
        }
    }
}
