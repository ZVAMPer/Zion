using UnityEngine;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Health Settings")]
    [SerializeField]
    private int maxHealth = 100; // Maximum health value

    [SerializeField]
    NetworkVariable<int> currentHealth = new NetworkVariable<int>(100); // Current health value

    [Header("Death Settings")]
    [SerializeField]
    private GameObject deathEffectPrefab; // Prefab to instantiate upon death

    // Event triggered when the player takes damage
    public event Action<int, int> OnTakeDamage;

    // Event triggered when the player dies
    public event Action OnDeath;

    // Property to access current health

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        // Optionally, you can subscribe to events or initialize UI elements here
    }

    /// <summary>
    /// Method to apply damage to the player.
    /// This should be called by other scripts (e.g., WeaponRifle) when the player is hit.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply.</param>
    public void  TakeDamage(int damageAmount)
    {

        if (currentHealth.Value <= 0)
        {
            // Player is already dead
            return;
        }
        Debug.Log("TakeDamage");
        // Reduce health
        currentHealth.Value -= damageAmount;
        currentHealth.Value = Mathf.Max(currentHealth.Value, 0);

        // Trigger the OnTakeDamage event
        OnTakeDamage?.Invoke(currentHealth.Value, maxHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Remaining Health: {currentHealth}");

        if (currentHealth.Value <= 0)
        {
            HandleDeath();
        }
    }

    /// <summary>
    /// Handles player death logic.
    /// </summary>
    private void HandleDeath()
    {
        // Trigger the OnDeath event
        OnDeath?.Invoke();

        Debug.Log($"{gameObject.name} has died.");

        // Instantiate death effect if assigned
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Optionally, handle respawn or disable player controls here
        // For example:
        // Disable player movement, trigger respawn after a delay, etc.
    }

    /// <summary>
    /// Method to heal the player. Can be expanded as needed.
    /// </summary>
    /// <param name="healAmount">Amount of health to restore.</param>
    public void Heal(int healAmount)
    {
        if (currentHealth.Value <= 0)
        {
            // Cannot heal a dead player
            return;
        }

        currentHealth.Value += healAmount;
        currentHealth.Value = Mathf.Min(currentHealth.Value, maxHealth);

        // Trigger the OnTakeDamage event to update UI or other systems
        OnTakeDamage?.Invoke(currentHealth.Value, maxHealth);

        Debug.Log($"{gameObject.name} healed by {healAmount}. Current Health: {currentHealth}");
    }

    /// <summary>
    /// Server RPC to apply damage from clients.
    /// Ensures that only the server processes the damage.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply.</param>
    [ServerRpc]
    public void ApplyDamageServerRpc(int damageAmount)
    {
        Debug.Log($"Received damage request from client: {damageAmount}");
        Debug.Log("Applying damage on server.");
        TakeDamage(damageAmount);
    }

    /// <summary>
    /// Optional: Override OnNetworkSpawn to initialize health based on network state.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        // Synchronize health with clients
        UpdateHealthClientRpc(currentHealth.Value, maxHealth);
    }

    /// <summary>
    /// Client RPC to update health on all clients.
    /// </summary>
    /// <param name="health">Current health value.</param>
    /// <param name="maxHealthValue">Maximum health value.</param>
    [ClientRpc]
    private void UpdateHealthClientRpc(int health, int maxHealthValue)
    {
        currentHealth.Value = health;
        // Update UI or other client-side elements here
        // For example, update a health bar
    }
}