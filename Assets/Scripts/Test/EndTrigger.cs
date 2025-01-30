using UnityEngine;
using Unity.Netcode;

public class EndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || (other.transform.parent != null && other.transform.parent.CompareTag("Player")))
        {
            NetworkObject networkObject = other.GetComponent<NetworkObject>();
            if (networkObject == null && other.transform.parent != null)
            {
                networkObject = other.transform.parent.GetComponent<NetworkObject>();
            }

            Debug.Log("EndTrigger: Player entered the trigger");
            if (networkObject != null && networkObject.IsOwner)
            {
                RaceManager.Instance.PlayerWonServerRpc(networkObject.OwnerClientId);
            }
        }
    }
}