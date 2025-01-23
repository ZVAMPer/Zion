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
            if (_surfCharacter != null)
            {
                _surfCharacter.Reset();
            }
            else
            {
                // Fallback for non-SurfCharacter objects
                transform.SetPositionAndRotation(_spawnPosition, Quaternion.identity);
                GetComponent<NetworkTransform>().Teleport(
                    _spawnPosition,
                    Quaternion.identity,
                    Vector3.one
                );
            }
        }
    }
}
