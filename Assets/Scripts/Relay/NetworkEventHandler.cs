
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkEventHandler : MonoBehaviour
{
    public TMP_Text statusText;

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && clientId == NetworkManager.Singleton.LocalClientId)
        {
            // Host has started
            statusText.text = "Hosting started.";
        }
        else
        {
            statusText.text = $"Client connected: {clientId}";
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            statusText.text = "You have been disconnected.";
        }
        else
        {
            statusText.text = $"Client disconnected: {clientId}";
        }
    }

    private void OnServerStarted()
    {
        statusText.text = "Server started.";
    }
}