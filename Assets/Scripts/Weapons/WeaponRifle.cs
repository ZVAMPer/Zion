using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System;

public class WeaponRifle : WeaponBase
{
    // Configuration Parameters
    [Header("Magazine Settings")]
    public int magazineSize = 30;           // Total bullets in a magazine
    public int bulletCount;                 // Current bullets in the magazine

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


    // State Variables
    private bool isFiring = false;           // Is the player holding the fire input
    private bool isReloading = false;        // Is the weapon currently reloading
    private float lastFireTime = 0f;         // Timestamp of the last burst
    private float lastReloadAttempt = -Mathf.Infinity; // Timestamp of the last reload attempt

    // References
    private Camera playerCamera;             // Reference to the player's camera
    [SerializeField]
    private TMPro.TMP_Text bulletCountText;  // Reference to the UI Text component

    // Initialization
    void Start()
    {
        bulletCount = magazineSize; // Initialize the magazine

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
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        HandleAutomaticReload();
        bulletCountText.text = bulletCount.ToString(); // Update the UI text
    }

    /// <summary>
    /// Handles player input for firing and reloading.
    /// </summary>
    private void HandleInput()
    {
        if (!IsOwner) {
            return;
        }
        // Check if the player is holding the fire input (e.g., left mouse button or a specific key)
        // Replace "Fire1" with your actual input axis or key
        isFiring = Input.GetButton("Fire1");

        // Check for manual reload input (e.g., pressing 'R')
        if (Input.GetButtonDown("Reload"))
        {
            AttemptReload();
        }

        // Handle firing logic
        if (isFiring && !isReloading && bulletCount > 0)
        {
            if (Time.time - lastFireTime >= fireRate)
            {
                FireBurst();
                lastFireTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Handles automatic reload when the magazine is empty.
    /// </summary>
    private void HandleAutomaticReload()
    {
        if (bulletCount <= 0 && !isReloading)
        {
            if (Time.time - lastReloadAttempt >= reloadCooldown)
            {
                StartCoroutine(Reload());
                lastReloadAttempt = Time.time;
            }
        }
    }

    /// <summary>
    /// Attempts to start reloading if cooldown has passed.
    /// </summary>
    private void AttemptReload()
    {
        if (!isReloading && Time.time - lastReloadAttempt >= reloadCooldown && bulletCount < magazineSize)
        {
            StartCoroutine(Reload());
            lastReloadAttempt = Time.time;
        }
    }

    /// <summary>
    /// Fires a burst of bullets.
    /// </summary>
    private void FireBurst()
    {
        int bulletsToFire = Mathf.Min(burstCount, bulletCount);
        for (int i = 0; i < bulletsToFire; i++)
        {
            FireSingleBullet();
        }
    }

    /// <summary>
    /// Fires a single bullet using raycasting and renders a trail.
    /// </summary>
    private void FireSingleBullet()
    {
        bulletCount--;
        Debug.Log("Fired a bullet. Remaining: " + bulletCount);

        if (playerCamera == null)
        {
            Debug.LogError("Player camera not assigned. Cannot perform raycast.");
            return;
        }

        // Define the origin and direction of the ray
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        // Perform the raycast
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rayOrigin, rayDirection, out hit, bulletRange);

        if (hasHit)
        {
            string hitObjectName = hit.collider.name;
            Debug.Log("Hit: " + hitObjectName);

            // Check if the hit object has a PlayerHealth component or adjust based on your player identification
            // PlayerHealth player = hit.collider.GetComponent<PlayerHealth>();
            // if (player != null)
            // {
            //     player.TakeDamage(10); // Example damage value
            // }

            if (hitObjectName == "PlayerCollider")
            {
                Debug.Log("Player hit!");
                PlayerHealth playerHealth = hit.collider.gameObject.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // Apply damage via ServerRpc
                    Debug.Log("Applying damage to player: " + playerHealth.gameObject.name);
                    playerHealth.TakeDamageServerRpc(10); // Example damage value
                }
            }
        }

        // Determine the end point of the trail
        Vector3 endPoint = hasHit ? hit.point : rayOrigin + rayDirection * bulletRange;

        // Draw the bullet trail from muzzle point to hit point or max range
        if (bulletTrailPrefab != null)
        {
            FireSingleBulletServerRpc(muzzlePoint.position, endPoint);
        } 
    }
    

    [ServerRpc]
    private void FireSingleBulletServerRpc(Vector3 origin, Vector3 destination)
    {
        StartCoroutine(DrawTrail(origin, destination));
    }
    /// <summary>
    /// Coroutine to draw a bullet trail from origin to destination using TrailRenderer.
    /// </summary>
    /// <param name="origin">Start position of the trail (muzzle point).</param>
    /// <param name="destination">End position of the trail (hit point or max range).</param>
    /// <returns></returns>
    private IEnumerator DrawTrail(Vector3 origin, Vector3 destination)
    {
        // Instantiate the trail prefab at the muzzle point
        GameObject trail = Instantiate(bulletTrailPrefab, origin, Quaternion.identity);
        var instanceNetworkObject = trail.GetComponent<NetworkObject>();
        instanceNetworkObject.Spawn(); // Only sever can spawn objects

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

        // Wait for the reload time to simulate reloading
        yield return new WaitForSeconds(reloadTime);

        bulletCount = magazineSize;
        isReloading = false;
        Debug.Log("Reloaded. Bullets available: " + bulletCount);
    }

    /// <summary>
    /// Overrides the UseWeapon method from WeaponBase.
    /// </summary>
    public override void UseWeapon()
    {
        // This method can be used to trigger firing from other scripts
        // isFiring = true;
    }

    /// <summary>
    /// Overrides the aimationCode property from WeaponBase.
    /// </summary>
    public override int aimationCode { get => 1; }
}