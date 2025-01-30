using UnityEngine;
using System.Collections;
using Unity.Netcode;

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
                FireBurstServerRpc();
                lastFireTime = Time.time;
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
    [ServerRpc]
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
    [ServerRpc]
    private void FireBurstServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isReloading || bulletCount.Value <= 0)
            return;

        FireBurst();
    }

    /// <summary>
    /// Fires a burst of bullets.
    /// </summary>
    private void FireBurst()
    {
        int bulletsToFire = Mathf.Min(burstCount, bulletCount.Value);
        for (int i = 0; i < bulletsToFire; i++)
        {
            FireSingleBullet();
        }

        // Notify all clients to play firing effects
        PlayFireEffectsClientRpc();
    }

    /// <summary>
    /// Fires a single bullet using raycasting and renders a trail.
    /// </summary>
    private void FireSingleBullet()
    {
        bulletCount.Value--;
        Debug.Log("Fired a bullet. Remaining: " + bulletCount.Value);

        if (playerCamera == null)
        {
            Debug.LogError("Player camera not assigned. Cannot perform raycast.");
            return;
        }

        // Define the origin and direction of the ray
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        // Perform the raycast on the server
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rayOrigin, rayDirection, out hit, bulletRange, playerLayerMask);

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
        Vector3 endPoint = hasHit ? hit.point : rayOrigin + rayDirection * bulletRange;

        // Notify clients to draw the bullet trail
        DrawTrailClientRpc(muzzlePoint.position, endPoint);
    }

    /// <summary>
    /// ClientRPC to draw the bullet trail on all clients.
    /// </summary>
    [ClientRpc]
    private void DrawTrailClientRpc(Vector3 origin, Vector3 destination)
    {
        if (bulletTrailPrefab != null && muzzlePoint != null)
        {
            StartCoroutine(DrawTrail(origin, destination));
        }
    }

    /// <summary>
    /// Coroutine to draw a bullet trail from origin to destination using TrailRenderer.
    /// </summary>
    private IEnumerator DrawTrail(Vector3 origin, Vector3 destination)
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
    /// Coroutine to handle the reloading process.
    /// </summary>
    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        // Notify all clients to play reload effects
        PlayReloadEffectsClientRpc();

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
            FireBurstServerRpc();
        }
    }

    /// <summary>
    /// Overrides the aimationCode property from WeaponBase.
    /// </summary>
    public override int aimationCode { get => 1; }

    /// <summary>
    /// ClientRPC to play firing effects on all clients.
    /// </summary>
    [ClientRpc]
    private void PlayFireEffectsClientRpc()
    {
        // Implement firing visual and audio effects
        // Example:
        // muzzleFlash.Play();
        // audioSource.PlayOneShot(fireSound);
    }

    /// <summary>
    /// ClientRPC to play reloading effects on all clients.
    /// </summary>
    [ClientRpc]
    private void PlayReloadEffectsClientRpc()
    {
        // Implement reloading visual and audio effects
        // Example:
        // reloadAnimation.Play();
        // audioSource.PlayOneShot(reloadSound);
    }
}
