using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Camera))] // Ensures a Camera component is present
public class OwnerCameraDisable : NetworkBehaviour
{
    [SerializeField] private AudioListener _audioListener;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (!IsOwner)
        {
            // Disable Camera and AudioListener
            if (TryGetComponent(out Camera cam))
                cam.enabled = false;
            if (_audioListener != null)
                _audioListener.enabled = false;
        }
    }
}
