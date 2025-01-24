using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using System;
using Unity.VisualScripting;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private string joinCode;
    private Allocation allocation;
    private JoinAllocation joinAllocation;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Starts hosting a new game by creating a Relay allocation and starting the host.
    /// </summary>
    public async Task<string> StartHost()
    {
        try
        {
            // Create a Relay allocation with a maximum number of players
            allocation = await RelayService.Instance.CreateAllocationAsync(8); // Adjust maxPlayers as needed

            // Get the join code for clients to connect
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Configure the Unity Transport with Relay parameters
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            // Start the host
            NetworkManager.Singleton.StartHost();
            Debug.Log($"Host started with join code: {joinCode}");

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            // Wait for a few seconds before retrying
            Debug.LogError($"Relay Host Start Failed: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Joins an existing game using a join code.
    /// </summary>
    /// <param name="code">The join code of the host to join.</param>
    public async Task<bool> JoinGame(string code)
    {
        try
        {
            // Join the Relay allocation using the provided join code
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            // Configure the Unity Transport with Relay parameters
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            // Start the client
            NetworkManager.Singleton.StartClient();
            Debug.Log($"Client joined with code: {code}");

            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Join Failed: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Leaves the current game, whether hosting or joining.
    /// </summary>
    public void LeaveGame()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            // If client, stop the client
            NetworkManager.Singleton.Shutdown();
            joinAllocation = null;
            Debug.Log("Client has left the game.");
        }
    }

    /// <summary>
    /// Retrieves the current join code if hosting.
    /// </summary>
    public string GetJoinCode()
    {
        return joinCode;
    }

    /// <summary>
    /// Checks if the player is currently hosting.
    /// </summary>
    public bool IsHosting()
    {
        return NetworkManager.Singleton.IsHost;
    }

    /// <summary>
    /// Checks if the player is currently a client.
    /// </summary>
    public bool IsClient()
    {
        return NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost;
    }
}