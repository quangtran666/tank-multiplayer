using Unity.Netcode;
using UnityEngine;

public class GameHUD : MonoBehaviour
{
    public async void LeaveGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            await HostSingleton.Instance.HostGameManager.ShutDown();
        }

        ClientSingleton.Instance.ClientGameManager.Disconnect();
    }
}
