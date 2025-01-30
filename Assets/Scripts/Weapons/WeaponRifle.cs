using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class WeaponRifle : WeaponBase  
{
    [Header("Magazine Settings")]
    public int magazineSize = 30;           
    private int currentBullets;

    [Header("Firing Settings")]
    public float fireRate = 0.1f;           
    public int burstCount = 3;              
    public float bulletRange = 100f;        
    public float trailDuration = 0.5f;      

    [Header("Reload Settings")]
    public float reloadCooldown = 2f;       
    public float reloadTime = 1.5f;         

    [Header("Trail Settings")]
    public GameObject bulletTrailPrefab;     
    public Transform muzzlePoint;            

    [Header("Layer Settings")]
    public LayerMask playerLayerMask;        

    private bool isFiring = false;           
    private bool isReloading = false;        
    private float lastFireTime = 0f;         
    private float lastReloadAttempt = -Mathf.Infinity;

    private Camera playerCamera;             
    private NetworkObject parentNetworkObject;
    private bool isLocalPlayer;

    void Start()
    {
        currentBullets = magazineSize;
        
        if (muzzlePoint == null)
        {
            muzzlePoint = transform;
            Debug.LogWarning("Muzzle Point not set. Using weapon's transform as default.");
        }

        parentNetworkObject = GetComponentInParent<NetworkObject>();
        isLocalPlayer = parentNetworkObject != null && parentNetworkObject.IsOwner;

        // Only set up camera for local player
        if (isLocalPlayer)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("Main Camera not found!");
            }
        }
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            HandleInput();
            HandleAutomaticReload();
        }
    }

    private void HandleInput()
    {
        isFiring = Input.GetButton("Fire1");

        if (isFiring && !isReloading && currentBullets > 0)
        {
            if (Time.time - lastFireTime >= fireRate)
            {
                if (playerCamera != null)
                {
                    Vector3 origin = playerCamera.transform.position;
                    Vector3 direction = playerCamera.transform.forward;
                    RequestFireServerRpc(origin, direction);
                    lastFireTime = Time.time;
                }
            }
        }

        if (Input.GetButtonDown("Reload"))
        {
            RequestReloadServerRpc();
        }
    }

    private void HandleAutomaticReload()
    {
        if (currentBullets <= 0 && !isReloading)
        {
            if (Time.time - lastReloadAttempt >= reloadCooldown)
            {
                RequestReloadServerRpc();
                lastReloadAttempt = Time.time;
            }
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestFireServerRpc(Vector3 origin, Vector3 direction)
    {
        if (isReloading || currentBullets <= 0) return;

        // Perform server-side validation if needed
        currentBullets--;
        
        // Perform the raycast on the server
        RaycastHit hit;
        bool hasHit = Physics.Raycast(origin, direction, out hit, bulletRange, playerLayerMask);
        Vector3 endPoint = hasHit ? hit.point : origin + direction * bulletRange;

        // Tell all clients to show the effects
        FireEffectsClientRpc(origin, endPoint, parentNetworkObject.OwnerClientId);
    }

    [ClientRpc]
    private void FireEffectsClientRpc(Vector3 origin, Vector3 endPoint, ulong shooterClientId)
    {
        // Don't show trail for the shooter's local view (they'll see it from their camera)
        if (parentNetworkObject.OwnerClientId == shooterClientId && isLocalPlayer)
            return;

        StartCoroutine(DrawTrail(origin, endPoint));
    }

    private IEnumerator DrawTrail(Vector3 origin, Vector3 destination)
    {
        GameObject trail = Instantiate(bulletTrailPrefab, origin, Quaternion.identity);
        TrailRenderer trailRenderer = trail.GetComponent<TrailRenderer>();
        
        if (trailRenderer != null)
        {
            float distance = Vector3.Distance(origin, destination);
            Vector3 direction = (destination - origin).normalized;
            float moveSpeed = distance / 0.1f; // Complete trail in 0.1 seconds

            float startTime = Time.time;
            float journeyLength = Vector3.Distance(origin, destination);
            float distanceCovered = 0f;

            while (distanceCovered < journeyLength)
            {
                float currentDuration = (Time.time - startTime);
                distanceCovered = currentDuration * moveSpeed;
                float fractionOfJourney = distanceCovered / journeyLength;

                trail.transform.position = Vector3.Lerp(origin, destination, fractionOfJourney);
                yield return null;
            }

            yield return new WaitForSeconds(trailDuration);
            Destroy(trail);
        }
    }

    [ServerRpc(RequireOwnership = true)]
    private void RequestReloadServerRpc()
    {
        if (!isReloading && currentBullets < magazineSize)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        ReloadEffectsClientRpc();
        
        yield return new WaitForSeconds(reloadTime);
        
        currentBullets = magazineSize;
        isReloading = false;
        
        ReloadCompleteClientRpc(currentBullets);
    }

    [ClientRpc]
    private void ReloadEffectsClientRpc()
    {
        // Play reload effects/animations
    }

    [ClientRpc]
    private void ReloadCompleteClientRpc(int newBulletCount)
    {
        currentBullets = newBulletCount;
        // Update UI or other elements
    }

    public override void UseWeapon()
    {
        if (isLocalPlayer && !isReloading && currentBullets > 0)
        {
            if (playerCamera != null)
            {
                Vector3 origin = playerCamera.transform.position;
                Vector3 direction = playerCamera.transform.forward;
                RequestFireServerRpc(origin, direction);
            }
        }
    }

    public override int aimationCode => 1;
}
