using Fragsurf.Movement;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerDeath : NetworkBehaviour
{
    [SerializeField]
    public GameObject ragdollPrefab;
    void Awake()
    {
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void DieServerRpc()
    {
                // Trigger the player death sound:
        // If this is the owner, play the local death SFX.
        if (IsOwner)
        {
            AudioManager.Instance.PlayPlayerDieSFXLocal("PlayerDie");
        }
        // Send a networked RPC so remote clients play the death SFX as spatial audio.
        NetworkedAudioManager.Instance.PlayPlayerDieSFXServerRpc("PlayerDie", transform.position);

        
        ragdollPrefab = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        ragdollPrefab.GetComponent<NetworkObject>().Spawn();

        GetComponent<PlayerHealth>().ReviveServerRpc();
        GetComponent<Respawn>().RespawnServerRpc();
    }
}
