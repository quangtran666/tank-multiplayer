using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeReference] private ClientSingleton clientPrefab;
    [SerializeReference] private HostSingleton hostPrefab;

    async void Start()
    {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {

        }
        else
        {
            var hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            var clientSingleton = Instantiate(clientPrefab);
            var authenticated = await clientSingleton.CreateClient();

            if (authenticated)
            {
                clientSingleton.ClientGameManager.GoToMenu();
            }
        }
    }
}
