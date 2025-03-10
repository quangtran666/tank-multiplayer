using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private Transform lobbyItemContainer;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    private bool isJoiningLobby = false;
    private bool isRefreshingList = false;

    void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshingList)
        {
            Debug.Log("Already refreshing the lobby list. Please wait.");
            return;
        }

        isRefreshingList = true;

        try
        {
            var queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>()
            {
                new(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                ),
                new(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0"
                )
            }
            };

            var lobbies = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            foreach (Transform child in lobbyItemContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var lobby in lobbies.Results)
            {
                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemContainer);
                lobbyItem.Initialise(this, lobby);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError($"Failed to refresh lobby list: {ex.Message}");
        }
    }

    public async Task JoinAsync(Lobby lobby)
    {
        if (isJoiningLobby)
        {
            Debug.Log("Already joining a lobby. Please wait.");
            return;
        }

        isJoiningLobby = true;

        try
        {
            var joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
            var joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.ClientGameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
        }
        finally
        {
            isJoiningLobby = false;
        }
    }
}
