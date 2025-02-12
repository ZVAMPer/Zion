using UnityEngine;
using System.Collections;
using Unity.Netcode;
using System;

public class WeaponRifle : WeaponBase
{
    // Configuration Parameters (unchanged parts omitted for brevity)

    [Header("Magazine Settings")]
    public int magazineSize = 30;           // Total bullets in a magazine
    public int bulletCount;                 // Current bullets in the magazine

    [Header("Firing Settings")]
    public float fireRate = 0.1f;           // Time between bursts
    public int burstCount = 1;              // Number of bullets per burst

    [Header("Reload Settings")]
    public float reloadCooldown = 2f;       // Cooldown time before reloading
    public float reloadTime = 1.5f;         // Time it takes to reload

    [Header("Projectile Settings")]
    public GameObject bulletPrefab;         // Prefab for the projectile bullet
    public Transform muzzlePoint;           // Point from where the bullet is spawned
    public float projectileVelocity = 50f;  // Adjustable projectile speed

    // State Variables (unchanged)
    private bool isFiring = false;
    private bool isReloading = false;
    private float lastFireTime = 0f;
    private float lastReloadAttempt = -Mathf.Infinity;

    // References
    private Camera playerCamera;
    [SerializeField]
    private TMPro.TMP_Text bulletCountText;

    void Start()
    {
        bulletCount = magazineSize;

        if (muzzlePoint == null)
        {
            muzzlePoint = this.transform;
            Debug.LogWarning("Muzzle Point not set. Using weapon's transform as default.");
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is not assigned. Please assign it in the Inspector.");
        }

        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("Main Camera not found. Please ensure your player camera is tagged as 'MainCamera'.");
        }
    }

    void Update()
    {
        HandleInput();
        HandleAutomaticReload();
        bulletCountText.text = bulletCount.ToString();
    }

    private void HandleInput()
    {
        if (!IsOwner)
            return;

        isFiring = Input.GetButton("Fire1");

        if (Input.GetButtonDown("Reload"))
            AttemptReload();

        if (isFiring && !isReloading && bulletCount > 0)
        {
            if (Time.time - lastFireTime >= fireRate)
            {
                FireBurst();
                lastFireTime = Time.time;
            }
        }
    }

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

    private void AttemptReload()
    {
        if (!isReloading && Time.time - lastReloadAttempt >= reloadCooldown && bulletCount < magazineSize)
        {
            StartCoroutine(Reload());
            lastReloadAttempt = Time.time;
            AudioManager.Instance.PlayGunSFXLocal("GunReload");
            Vector3 shooterPos = (transform.parent != null) ? transform.parent.position : transform.position;
            NetworkedAudioManager.Instance.PlayGunReloadSFXServerRpc("GunReload", shooterPos);
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
    /// Fires a single bullet as a projectile.
    /// </summary>
    private void FireSingleBullet()
    {
        bulletCount--;
        AudioManager.Instance.PlayGunSFXLocal("GunShot");
        Vector3 shooterPos = (transform.parent != null) ? transform.parent.position : transform.position;
        NetworkedAudioManager.Instance.PlayGunSFXServerRpc("GunShot", shooterPos);

        // Use the player's camera forward direction as the aiming direction.
        Vector3 direction = playerCamera.transform.forward;

        // Spawn the projectile bullet using a ServerRpc.
        FireProjectileServerRpc(muzzlePoint.position, direction, projectileVelocity);
    }

    [ServerRpc]
    private void FireProjectileServerRpc(Vector3 origin, Vector3 direction, float velocity)
    {
        // Instantiate the projectile bullet prefab at the origin with the proper rotation.
        GameObject projectile = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(direction));
        
        // (Optional) Pass the desired projectile velocity to the bullet script.
        BulletProjectile projectileScript = projectile.GetComponent<BulletProjectile>();

        if (projectileScript != null)
        {
            projectileScript.projectileVelocity = velocity;
            projectileScript.owner = GetComponentInParent<PlayerHealth>();
        }

        // Spawn the projectile as a network object so all clients see it.
        projectile.GetComponent<NetworkObject>().Spawn();
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        bulletCount = magazineSize;
        isReloading = false;
        Debug.Log("Reloaded. Bullets available: " + bulletCount);
    }

    public override void UseWeapon()
    {
        // This method can be used to trigger firing from other scripts if needed.
    }

    public override int aimationCode { get => 1; }
}