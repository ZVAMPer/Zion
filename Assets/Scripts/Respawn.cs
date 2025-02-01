using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;

public class Respawn : NetworkBehaviour
{
    public float floorY = -300f;
    private Vector3 _spawnPosition;
    private Fragsurf.Movement.SurfCharacter _surfCharacter;
    private Vector3 _initialSpawnPosition;
    private Quaternion _initialSpawnRotation;

    GameObject respawnPointA;
    GameObject respawnPointB;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        _surfCharacter = GetComponent<Fragsurf.Movement.SurfCharacter>();

        //  Find two respawn points with tag "RespawnPoint"
        GameObject[] respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        if (respawnPoints.Length >= 2)
        {
            respawnPointA = respawnPoints[0];
            respawnPointB = respawnPoints[1];
        }
        
        if (respawnPointA != null && respawnPointB != null)
        {
            if (IsHost && IsOwner)
            {
                transform.position = respawnPointA.transform.position;
                transform.rotation = respawnPointA.transform.rotation;
            }
            else
            {
                transform.position = respawnPointB.transform.position;
                transform.rotation = respawnPointB.transform.rotation;
            }
        }

        if (_surfCharacter != null)
        {
            // Memorize the initial spawn position and rotation
            _initialSpawnPosition = transform.position;
            _initialSpawnRotation = transform.rotation;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (transform.position.y < floorY)
        {
            if (_surfCharacter != null)
            {
                // Use the memorized initial position and rotation for respawning
                _surfCharacter.ResetClientRpc(_initialSpawnPosition, _initialSpawnRotation);
            }
            else
            {
                // Fallback for non-SurfCharacter objects
                transform.SetPositionAndRotation(_initialSpawnPosition, _initialSpawnRotation);
                GetComponent<NetworkTransform>().Teleport(
                    _initialSpawnPosition,
                    _initialSpawnRotation,
                    Vector3.one
                );
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RespawnServerRpc()
    {
        if (_surfCharacter != null)
        {
            // Use the memorized initial position and rotation for respawning
            _surfCharacter.ResetClientRpc(_initialSpawnPosition, _initialSpawnRotation);
        }
        else
        {
            transform.SetPositionAndRotation(_initialSpawnPosition, _initialSpawnRotation);
            GetComponent<NetworkTransform>().Teleport(
                _initialSpawnPosition,
                _initialSpawnRotation,
                Vector3.one
            );
        }
    }

	[ClientRpc]
    public void ResetClientRpc()
    {
        if (IsServer)
        {
            if (_surfCharacter != null)
            {
                _surfCharacter.ResetClientRpc(_initialSpawnPosition, _initialSpawnRotation);
            }
            else
            {
                transform.SetPositionAndRotation(_initialSpawnPosition, _initialSpawnRotation);
                GetComponent<NetworkTransform>().Teleport(
                    _initialSpawnPosition,
                    _initialSpawnRotation,
                    Vector3.one
                );
            }
        }
    }
}
