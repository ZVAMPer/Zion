using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Respawn : NetworkBehaviour
{
    public float floorY = -300f;
    private Vector3 _spawnPosition;
    private Fragsurf.Movement.SurfCharacter _surfCharacter;
    private Vector3 _initialSpawnPosition;
    private Quaternion _initialSpawnRotation;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        _surfCharacter = GetComponent<Fragsurf.Movement.SurfCharacter>();
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

        if (!IsOwner) return;

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
}
