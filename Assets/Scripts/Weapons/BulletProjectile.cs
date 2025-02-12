using UnityEngine;
using Unity.Netcode;

public class BulletProjectile : NetworkBehaviour
{
    [Header("Projectile Settings")]
    public float projectileVelocity = 50f; // Speed at which the bullet moves.
    public float lifeTime = 5f;            // Time before the bullet is automatically despawned.
    public int damage = 10;                // Damage inflicted on a successful hit.
    public float knockbackForce = 40f;
    public PlayerHealth owner;             // Reference to the player who fired the bullet.

    private float spawnTime;

    [Header("Knockback Settings")]
    public float knockbackForwardBoost = 10f;    // Additional forward force applied to the hit player.
    public float knockbackUpwardBoost = 10f;       // Additional upward force applied.
    public float knockbackGravityMultiplier = 0.5f; // Gravity reduction factor while knockback is active.
    public float knockbackDuration = 1f;

    void Start()
    {
        spawnTime = Time.time;
        // Optional: Log the projectile's velocity for debugging.
        Debug.Log("Projectile started with velocity: " + projectileVelocity);
    }

    void Update()
    {
        // Move the bullet forward.
        transform.position += transform.forward * projectileVelocity * Time.deltaTime;

        // Only the server should handle despawning.
        if (IsServer && (Time.time - spawnTime > lifeTime))
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only process collisions on the server.
        if (!IsServer)
            return;

        // Apply damage if the object has a PlayerHealth component.
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == owner)
            return;
            
        if (playerHealth != null)
        {
            playerHealth.TakeDamageServerRpc(damage);
        }

        // If the hit object has a SurfCharacter component, apply knockback.
        Fragsurf.Movement.SurfCharacter character = other.GetComponentInParent<Fragsurf.Movement.SurfCharacter>();
        if (character != null)
        {
            // Compute the knockback velocity.
            // First, get the player's current horizontal velocity.
            Vector3 currentVelocity = character.moveData.velocity;
            currentVelocity.y = 0f; // ignore vertical speed for computing direction

            // Determine the launch direction: if the player is moving, use that direction;
            // otherwise, default to the bullet’s travel (forward) direction.
            Vector3 launchDir = (currentVelocity.magnitude > 0.1f)
                ? currentVelocity.normalized
                : transform.forward;

            // Combine the current velocity with a boost in the launch direction plus an upward boost.
            Vector3 knockbackVelocity = currentVelocity 
                + (launchDir * knockbackForwardBoost)
                + (Vector3.up * knockbackUpwardBoost);

            // Use a ClientRpc targeted to the hit player so that their local movement is updated.
            // (This assumes SurfCharacter has the ApplyKnockbackClientRpc method defined.)
            var targetClientId = character.GetComponent<NetworkObject>().OwnerClientId;
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { targetClientId } }
            };
            character.ApplyKnockbackClientRpc(knockbackVelocity, knockbackGravityMultiplier, knockbackDuration, clientRpcParams);
        }

        // Despawn the bullet.
        GetComponent<NetworkObject>().Despawn();
    }
}