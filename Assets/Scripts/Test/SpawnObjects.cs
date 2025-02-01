using UnityEngine;
using Unity.Netcode;

public class SpawnObjects : NetworkBehaviour
{
    public GameObject objectToSpawn; // Assign the prefab in the Inspector

    
    void Start()
    {
        // if (IsServer)
        // {
        //     Debug.Log("Server is starting, calling SpawnObjectServerRpc");
        //     SpawnObjectServerRpc();
        // }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            Debug.Log("Server is spawning, calling SpawnObjectServerRpc");
            SpawnObjectServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnObjectServerRpc()
    {
        Debug.Log("SpawnObjectServerRpc called");
        GameObject spawnedObject = Instantiate(objectToSpawn, this.transform.position, Quaternion.identity);
        spawnedObject.GetComponent<NetworkObject>().Spawn();
        Debug.Log("Object spawned and NetworkObject.Spawn called");
    }
}