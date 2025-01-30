using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;
using Unity.Services.Lobbies.Models;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    public int maxHealth = 100; // Maximum health value

    [SerializeField]
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100); // Current health value
    [SerializeField]
    private TMPro.TMP_Text healthText;

    PlayerDeath playerDeath;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerDeath = GetComponent<PlayerDeath>();
        currentHealth.Value = maxHealth;
        healthText.text = currentHealth.Value.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        currentHealth.Value -= damage;
        healthText.text = currentHealth.Value.ToString();
        if (currentHealth.Value <= 0)
        {
            playerDeath.DieServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReviveServerRpc()
    {
        currentHealth.Value = maxHealth;
        healthText.text = currentHealth.Value.ToString();
    }
}