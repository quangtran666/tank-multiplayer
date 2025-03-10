using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameManager : IAsyncDisposable
{
    private const string GameSceneName = "Game";

    private Allocation allocation;
    private const int MaxConnections = 20;
    private string joinCode;
    private string lobbyId;
    private NetworkServer networkServer;

    public async Task StartHostAsync()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(MaxConnections);
        }
        catch (RelayServiceException relayException)
        {
            Debug.LogError($"Relay service error: {relayException.Message}");
            return;
        }

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join code: {joinCode}");
        }
        catch (RelayServiceException relayException)
        {
            Debug.LogError($"Relay service error: {relayException.Message}");
            return;
        }

        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        var relayServerData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        unityTransport.SetRelayServerData(relayServerData);

        try
        {
            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "JoinCode", new DataObject(DataObject.VisibilityOptions.Member, joinCode)
                    }
                }
            };
            var playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing name lobby");
            var lobby = await LobbyService.Instance.CreateLobbyAsync($"{playerName}'s lobby", MaxConnections, lobbyOptions);
            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15f));
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Lobby service error: {ex.Message}");
            return;
        }

        var userData = new UserData
        {
            UserName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
            UserAuthID = AuthenticationService.Instance.PlayerId
        };
        var payload = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(userData));
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payload;

        networkServer = new NetworkServer(NetworkManager.Singleton);

        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds)
    {
        WaitForSecondsRealtime delay = new(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async ValueTask DisposeAsync()
    {
        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        if (!string.IsNullOrEmpty(lobbyId))
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError($"Failed to delete lobby: {ex.Message}");
            }

            lobbyId = string.Empty;
        }

        networkServer?.Dispose();
    }
}
