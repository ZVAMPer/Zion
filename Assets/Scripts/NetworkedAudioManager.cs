using UnityEngine;
using Unity.Netcode;

public class NetworkedAudioManager : NetworkBehaviour
{
    public static NetworkedAudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ServerRpc automatically captures the sender's client id and passes it along
    [ServerRpc(RequireOwnership = false)]
    public void PlayGunSFXServerRpc(string key, Vector3 shooterPosition, ServerRpcParams rpcParams = default)
    {
        ulong shooterId = rpcParams.Receive.SenderClientId;
        PlayGunSFXClientRpc(key, shooterPosition, shooterId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void PlayGunSFXClientRpc(string key, Vector3 shooterPosition, ulong shooterId)
    {
        // Only play spatial sound on remote clients, not on the shooter
        if (NetworkManager.Singleton.LocalClientId == shooterId)
            return;

        Vector3 localPlayerPosition = GetLocalPlayerPosition();
        Debug.Log($"Remote Gun SFX: Shooter client {shooterId} fired from {shooterPosition}. " +
                  $"Local client {NetworkManager.Singleton.LocalClientId} at position {localPlayerPosition}.");
        
        // Play the spatial sound using the caller's (shooter’s) position
        AudioManager.Instance.PlayGunSFXRemote(key, shooterPosition);
    }

    // Helper to retrieve a local player's position (using main camera as a placeholder)
    private Vector3 GetLocalPlayerPosition()
    {
        if (Camera.main != null)
            return Camera.main.transform.position;
        return Vector3.zero;
    }
}