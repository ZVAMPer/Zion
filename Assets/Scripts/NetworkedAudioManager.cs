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

        // Vector3 localPlayerPosition = GetLocalPlayerPosition();
        // Debug.Log($"Remote Gun SFX: Shooter client {shooterId} fired from {shooterPosition}. " +
        //           $"Local client {NetworkManager.Singleton.LocalClientId} at position {localPlayerPosition}.");

        // Play the spatial sound using the caller's (shooter’s) position
        AudioManager.Instance.PlayGunSFXRemote(key, shooterPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayPlayerStepsSFXServerRpc(string key, Vector3 position, ServerRpcParams rpcParams = default)
    {
        ulong callerId = rpcParams.Receive.SenderClientId;
        PlayPlayerStepsSFXClientRpc(key, position, callerId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void PlayPlayerStepsSFXClientRpc(string key, Vector3 position, ulong callerId)
    {
        // Only play spatial footstep sound on remote clients.
        if (NetworkManager.Singleton.LocalClientId == callerId)
            return;

        AudioManager.Instance.PlayPlayerStepsSFXRemote(key, position);
    }

    // New Reload SFX: ServerRpc automatically captures the sender's client id and passes it along
    [ServerRpc(RequireOwnership = false)]
    public void PlayGunReloadSFXServerRpc(string key, Vector3 shooterPosition, ServerRpcParams rpcParams = default)
    {
        ulong shooterId = rpcParams.Receive.SenderClientId;
        PlayGunReloadSFXClientRpc(key, shooterPosition, shooterId);
    }

    [ClientRpc]
    private void PlayGunReloadSFXClientRpc(string key, Vector3 shooterPosition, ulong shooterId)
    {
        // Do not play the reload sound on the shooter’s client
        if (NetworkManager.Singleton.LocalClientId == shooterId)
            return;

        // Play the reload sound as spatial audio on remote clients
        AudioManager.Instance.PlayGunSFXRemote(key, shooterPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayPlayerDieSFXServerRpc(string key, Vector3 position, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        PlayPlayerDieSFXClientRpc(key, position, senderId);
    }

    [ClientRpc]
    private void PlayPlayerDieSFXClientRpc(string key, Vector3 position, ulong senderId)
    {
        // Prevent playing the sound twice on the dying player's client.
        if (NetworkManager.Singleton.LocalClientId == senderId)
            return;

        AudioManager.Instance.PlayGunSFXRemote(key, position);
    }

    // Helper to retrieve a local player's position (using main camera as a placeholder)
    private Vector3 GetLocalPlayerPosition()
    {
        if (Camera.main != null)
            return Camera.main.transform.position;
        return Vector3.zero;
    }
}