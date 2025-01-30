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
        ragdollPrefab = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        ragdollPrefab.GetComponent<NetworkObject>().Spawn();

        GetComponent<PlayerHealth>().ReviveServerRpc();
        GetComponent<Respawn>().RespawnServerRpc();
    }
}
