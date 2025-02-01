using UnityEngine;
using Unity.Netcode.Components;
using Unity.Netcode;
using System.Collections;
using System.Runtime.ExceptionServices;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;
    public GameObject successUI;
    public GameObject loseUI;
    public Transform startPosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerWonServerRpc(ulong playerId)
    {
        StartCoroutine(HandleWin(playerId));
    }

    private IEnumerator HandleWin(ulong winnerId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == winnerId)
            {
                ShowSuccessUIClientRpc(client.ClientId);
            }
            else
            {
                ShowLoseUIClientRpc(client.ClientId);
            }
        }

        yield return new WaitForSeconds(5);

        HideUIForAllClients();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            TeleportPlayerToStartServerRpc(client.ClientId);
        }
    }

    [ClientRpc]
    private void ShowSuccessUIClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            successUI.SetActive(true);
            loseUI.SetActive(false);
        }
    }

    [ClientRpc]
    private void ShowLoseUIClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            successUI.SetActive(false);
            loseUI.SetActive(true);
        }
    }

    [ClientRpc]
    private void HideUIClientRpc()
    {
        successUI.SetActive(false);
        loseUI.SetActive(false);
    }

    private void HideUIForAllClients()
    {
        HideUIClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TeleportPlayerToStartServerRpc(ulong clientId)
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponentsInChildren<Respawn>())
        {
            if (player != null)
            {
                player.ResetClientRpc();

            }
            else
            {
                Debug.LogWarning($"Player object for client {clientId} not found.");
            }
        }

    }
}