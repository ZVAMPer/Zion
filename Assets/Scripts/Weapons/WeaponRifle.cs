using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class WeaponRifle : WeaponBase  
{
    // Configuration Parameters
    [Header("Magazine Settings")]
    public int magazineSize = 30;           // Total bullets in a magazine

    [Header("Firing Settings")]
    public float fireRate = 0.1f;           // Time between bursts
    public int burstCount = 3;              // Number of bullets per burst
    public float bulletRange = 100f;        // Maximum range of a bullet
    public float trailDuration = 0.5f;      // Duration for the bullet trail to be visible

    [Header("Reload Settings")]
    public float reloadCooldown = 2f;       // Cooldown time before reloading
    public float reloadTime = 1.5f;         // Time it takes to reload

    [Header("Trail Settings")]
    public GameObject bulletTrailPrefab;     // Prefab for the bullet trail
    public Transform muzzlePoint;            // Point from where the bullet trail is drawn

    [Header("Layer Settings")]
    public LayerMask playerLayerMask;        // Layer mask to detect players

    // State Variables
    private bool isFiring = false;           // Is the player holding the fire input
    private bool isReloading = false;        // Is the weapon currently reloading
    private float lastFireTime = 0f;         // Timestamp of the last burst
    private float lastReloadAttempt = -Mathf.Infinity; // Timestamp of the last reload attempt

    // References
    private Camera playerCamera;             // Reference to the player's camera

    // Networked Variables
    public NetworkVariable<int> bulletCount = new NetworkVariable<int>();

    // Initialization
    void Start()
    {
        if (IsServer)
        {
            bulletCount.Value = magazineSize; // Initialize the magazine on the server
        }

        // Validate references
        if (muzzlePoint == null)
        {
            muzzlePoint = this.transform; // Default to weapon's transform if not set
            Debug.LogWarning("Muzzle Point not set. Using weapon's transform as default.");
        }

        if (bulletTrailPrefab == null)
        {
            Debug.LogError("Bullet Trail Prefab is not assigned. Please assign it in the Inspector.");
        }

        // Get the main camera (assuming the player's camera is tagged as MainCamera)
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("Main Camera not found. Please ensure your player camera is tagged as 'MainCamera'.");
        }

        // Subscribe to bullet count changes for UI updates
        bulletCount.OnValueChanged += OnBulletCountChanged;
    }

    void OnDestroy()
    {
        bulletCount.OnValueChanged -= OnBulletCountChanged;
    }

    void OnBulletCountChanged(int oldCount, int newCount)
    {
        // Update the UI with the new bullet count
        // Example:
        // bulletUI.text = newCount.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            HandleInput();
            HandleAutomaticReload();
        }
    }

    /// <summary>
    /// Handles player input for firing and reloading.
    /// </summary>
    private void HandleInput()
    {
        // Check if the player is holding the fire input (e.g., left mouse button or a specific key)
        // Replace "Fire1" with your actual input axis or key
        isFiring = Input.GetButton("Fire1");

        // Handle firing logic
        if (isFiring && !isReloading && bulletCount.Value > 0)
        {
            if (Time.time - lastFireTime >= fireRate)
            {
                // Ensure playerCamera is not null
                if (playerCamera != null)
                {
                    Vector3 origin = playerCamera.transform.position;
                    Vector3 direction = playerCamera.transform.forward;
                    FireBurstServerRpc(origin, direction);

                    // Play firing effects locally on the owner
                    PlayFireEffectsLocal();
                    DrawTrailLocal(origin, origin + direction * bulletRange);

                    lastFireTime = Time.time;
                }
                else
                {
                    Debug.LogError("Player camera is null. Cannot fire.");
                }
            }
        }

        // Check for manual reload input (e.g., pressing 'R')
        if (Input.GetButtonDown("Reload"))
        {
            AttemptReloadServerRpc();
        }
    }

    /// <summary>
    /// Handles automatic reload when the magazine is empty.
    /// </summary>
    private void HandleAutomaticReload()
    {
        if (bulletCount.Value <= 0 && !isReloading)
        {
            if (Time.time - lastReloadAttempt >= reloadCooldown)
            {
                AttemptReloadServerRpc();
                lastReloadAttempt = Time.time;
            }
        }
    }

    /// <summary>
    /// ServerRPC to attempt reloading.
    /// </summary>
    [ServerRpc(RequireOwnership = true)]
    private void AttemptReloadServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!isReloading && bulletCount.Value < magazineSize)
        {
            StartCoroutine(Reload());
            lastReloadAttempt = Time.time;
        }
    }

    /// <summary>
    /// ServerRPC to fire a burst.
    /// </summary>
    [ServerRpc(RequireOwnership = true)]
    private void FireBurstServerRpc(Vector3 origin, Vector3 direction, ServerRpcParams rpcParams = default)
    {
        if (isReloading || bulletCount.Value <= 0)
            return;

        FireBurst(origin, direction);

        // Prepare ClientRpcParams to exclude the owner
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        List<ulong> targetClientIds = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        targetClientIds.Remove(senderClientId);

        if (targetClientIds.Count > 0)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = targetClientIds.ToArray()
                }
            };

            // Notify other clients to play firing effects
            PlayFireEffectsClientRpc(clientRpcParams);

            // For each bullet fired, determine the end point and notify clients to draw the trail
            for (int i = 0; i < Mathf.Min(burstCount, bulletCount.Value + burstCount); i++)
            {
                Vector3 currentDirection = direction; // Adjust for spread if any
                RaycastHit hit;
                bool hasHit = Physics.Raycast(origin, currentDirection, out hit, bulletRange, playerLayerMask);

                Vector3 endPoint = hasHit ? hit.point : origin + currentDirection * bulletRange;

                // Notify other clients to draw the bullet trail
                DrawTrailClientRpc(origin, endPoint, clientRpcParams);
            }
        }
    }

    /// <summary>
    /// Fires a burst of bullets.
    /// </summary>
    private void FireBurst(Vector3 origin, Vector3 direction)
    {
        int bulletsToFire = Mathf.Min(burstCount, bulletCount.Value);
        for (int i = 0; i < bulletsToFire; i++)
        {
            FireSingleBullet(origin, direction);
        }

        // Notify all clients to play firing effects (handled via ClientRpc)
    }

    /// <summary>
    /// Fires a single bullet using raycasting and renders a trail.
    /// </summary>
    private void FireSingleBullet(Vector3 origin, Vector3 direction)
    {
        bulletCount.Value--;
        Debug.Log("Fired a bullet. Remaining: " + bulletCount.Value);

        // Perform the raycast on the server using the provided origin and direction
        RaycastHit hit;
        bool hasHit = Physics.Raycast(origin, direction, out hit, bulletRange, playerLayerMask);

        if (hasHit)
        {
            Debug.Log("Hit: " + hit.collider.name);

            // Example: Apply damage if the hit object has a PlayerHealth component
            // PlayerHealth player = hit.collider.GetComponent<PlayerHealth>();
            // if (player != null)
            // {
            //     player.TakeDamage(10); // Example damage value
            // }
        }

        // Determine the end point of the trail
        Vector3 endPoint = hasHit ? hit.point : origin + direction * bulletRange;

        // Notify clients to draw the bullet trail (handled in FireBurstServerRpc)
    }

    /// <summary>
    /// ClientRPC to play firing effects on all clients except the owner.
    /// </summary>
    [ClientRpc]
    private void PlayFireEffectsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Implement firing visual and audio effects
        // Example:
        // muzzleFlash.Play();
        // audioSource.PlayOneShot(fireSound);
    }

    /// <summary>
    /// ClientRPC to draw the bullet trail on all clients except the owner.
    /// </summary>
    [ClientRpc]
    private void DrawTrailClientRpc(Vector3 origin, Vector3 destination, ClientRpcParams clientRpcParams = default)
    {
        if (bulletTrailPrefab != null && muzzlePoint != null)
        {
            StartCoroutine(DrawTrailCoroutine(origin, destination));
        }
    }

    /// <summary>
    /// Coroutine to draw a bullet trail from origin to destination using TrailRenderer.
    /// </summary>
    private IEnumerator DrawTrailCoroutine(Vector3 origin, Vector3 destination)
    {
        // Instantiate the trail prefab at the muzzle point
        GameObject trail = Instantiate(bulletTrailPrefab, origin, Quaternion.identity);

        // Get the TrailRenderer component
        TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            // Set the initial position
            trail.transform.position = origin;

            // Calculate the direction and distance to move the trail
            Vector3 direction = (destination - origin).normalized;
            float distance = Vector3.Distance(origin, destination);

            // Move the trail to the destination over a short duration
            float moveDuration = trailDuration; // Adjust as needed
            float elapsedTime = 0f;

            while (elapsedTime < moveDuration)
            {
                trail.transform.position += direction * (distance / moveDuration) * Time.deltaTime;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the trail reaches the exact destination
            trail.transform.position = destination;
        }
        else
        {
            Debug.LogError("Bullet Trail Prefab does not have a TrailRenderer component.");
        }

        // Wait for the trail duration before destroying
        yield return new WaitForSeconds(trailDuration);

        // Destroy the trail after the duration
        Destroy(trail);
    }

    /// <summary>
    /// Handles the local firing effects for the owner client.
    /// </summary>
    private void PlayFireEffectsLocal()
    {
        // Implement local firing visual and audio effects
        // Example:
        // muzzleFlash.Play();
        // audioSource.PlayOneShot(fireSound);
    }

    /// <summary>
    /// Handles the local bullet trail for the owner client.
    /// </summary>
    private void DrawTrailLocal(Vector3 origin, Vector3 destination)
    {
        if (bulletTrailPrefab != null && muzzlePoint != null)
        {
            StartCoroutine(DrawTrailCoroutine(origin, destination));
        }
    }

    /// <summary>
    /// Coroutine to handle the reloading process.
    /// </summary>
    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        // Prepare ClientRpcParams to exclude the owner
        List<ulong> targetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(id => id != OwnerClientId).ToList();

        if (targetClientIds.Count > 0)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = targetClientIds.ToArray()
                }
            };

            // Notify other clients to play reload effects
            PlayReloadEffectsClientRpc(clientRpcParams);
        }

        // Play reload effects locally on the owner
        PlayReloadEffectsLocal();

        // Wait for the reload time to simulate reloading
        yield return new WaitForSeconds(reloadTime);

        bulletCount.Value = magazineSize;
        isReloading = false;
        Debug.Log("Reloaded. Bullets available: " + bulletCount.Value);
    }

    /// <summary>
    /// Overrides the UseWeapon method from WeaponBase.
    /// </summary>
    public override void UseWeapon()
    {
        if (IsOwner && !isReloading && bulletCount.Value > 0)
        {
            // Ensure playerCamera is not null
            if (playerCamera != null)
            {
                Vector3 origin = playerCamera.transform.position;
                Vector3 direction = playerCamera.transform.forward;
                FireBurstServerRpc(origin, direction);

                // Play firing effects locally on the owner
                PlayFireEffectsLocal();
                DrawTrailLocal(origin, origin + direction * bulletRange);
            }
            else
            {
                Debug.LogError("Player camera is null. Cannot use weapon.");
            }
        }
    }

    /// <summary>
    /// Overrides the animationCode property from WeaponBase.
    /// </summary>
    public override int aimationCode { get => 1; }

    /// <summary>
    /// ClientRPC to play reloading effects on all clients except the owner.
    /// </summary>
    [ClientRpc]
    private void PlayReloadEffectsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // Implement reloading visual and audio effects
        // Example:
        // reloadAnimation.Play();
        // audioSource.PlayOneShot(reloadSound);
    }

    /// <summary>
    /// Handles the local reloading effects for the owner client.
    /// </summary>
    private void PlayReloadEffectsLocal()
    {
        // Implement local reloading visual and audio effects
        // Example:
        // reloadAnimation.Play();
        // audioSource.PlayOneShot(reloadSound);
    }
}