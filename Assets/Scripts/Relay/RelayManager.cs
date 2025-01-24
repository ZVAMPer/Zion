using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private string joinCode;

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

    // Host a Relay server and return the join code
    public async Task<string> StartHost()
    {
        try
        {
            
            
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4); // Adjust maxPlayers as needed

            // Get the join code for clients to connect
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Configure the transport with Relay parameters
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
            Debug.LogError($"Relay Host Start Failed: {e.Message}");
            return null;
        }
    }

    // Join a Relay server using a join code
    public async Task JoinGame(string code)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);

            // Configure the transport with Relay parameters
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
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay Join Failed: {e.Message}");
        }
    }

    // Retrieve the current join code
    public string GetJoinCode()
    {
        return joinCode;
    }
}