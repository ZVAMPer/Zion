using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    public int maxHealth = 100; // Maximum health value

    [SerializeField]
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100); // Current health value

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int damage)
    {
        currentHealth.Value -= damage;
    }
}