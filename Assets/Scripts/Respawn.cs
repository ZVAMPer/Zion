using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Respawn : NetworkBehaviour
{
    public float floorY = -300f;
    private Vector3 _spawnPosition;
    private Fragsurf.Movement.SurfCharacter _surfCharacter;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;

        _spawnPosition = transform.position + Vector3.up * 5f;
        _surfCharacter = GetComponent<Fragsurf.Movement.SurfCharacter>();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (transform.position.y < floorY)
        {
            // Use SurfCharacter's reset method or directly modify moveData
            if (_surfCharacter != null)
            {
                _surfCharacter.ResetPosition(); // Call server-side reset
            }
            else
            {
                // Fallback: Update transform and notify NetworkTransform
                transform.position = _spawnPosition;
                GetComponent<NetworkTransform>().Teleport(
                    _spawnPosition,
                    transform.rotation,
                    Vector3.one
                );
            }
        }
    }
}

