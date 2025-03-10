using System;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;

    public ClientGameManager ClientGameManager { get; private set; }

    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindFirstObjectByType<ClientSingleton>();

            if (instance == null)
            {
                Debug.LogError("ClientSingleton instance not found in the scene. Creating a new one.");
                return null;
            }

            return instance;
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        ClientGameManager?.Dispose();
    }

    public async Task<bool> CreateClient()
    {
        ClientGameManager = new ClientGameManager();

        return await ClientGameManager.InitAsync();
    }
}
