using Fragsurf.Movement;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerDeath : NetworkBehaviour
{
    NetworkObject networkObject;
    NetworkTransform networkTransform;
    SurfCharacter surfCharacter;
    Respawn respawn;

    Rigidbody rb;

    NetworkRigidbody networkRigidbody;

    NetworkAnimator networkAnimator;

    WeaponManager weaponManager;
    PlayerHealth playerHealth;

    PlayerAiming playerAiming;

    Animator animator;

    WeaponRifle weaponRifle;

    CapsuleCollider capsuleCollider;

    [SerializeField]

    GameObject armature;

    // Awake is called when the script instance is being loaded
    void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
        networkTransform = GetComponent<NetworkTransform>();
        surfCharacter = GetComponent<SurfCharacter>();
        respawn = GetComponent<Respawn>();
        rb = GetComponent<Rigidbody>();
        networkRigidbody = GetComponent<NetworkRigidbody>();
        networkAnimator = GetComponent<NetworkAnimator>();
        weaponManager = GetComponent<WeaponManager>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAiming = GetComponentInChildren<PlayerAiming>();
        animator = GetComponentInChildren<Animator>();
        weaponRifle = GetComponentInChildren<WeaponRifle>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DieServerRpc()
    {
        //  Disable every component that is not needed when the player is dead
        networkObject.enabled = false;
        networkTransform.enabled = false;
        surfCharacter.enabled = false;
        respawn.enabled = false;
        rb.isKinematic = true;
        networkRigidbody.enabled = false;
        networkAnimator.enabled = false;
        weaponManager.enabled = false;
        playerHealth.enabled = false;
        playerAiming.enabled = false;
        armature.SetActive(true);
        animator.enabled = false;
        weaponRifle.enabled = false;
        capsuleCollider.enabled = false;


        //  Revive the player after 5 seconds
        // Invoke(nameof(Revive), 5f);
    }


    public void Die()
    {
        //  Disable every component that is not needed when the player is dead
        networkObject.enabled = false;
        networkTransform.enabled = false;
        surfCharacter.enabled = false;
        respawn.enabled = false;
        rb.isKinematic = true;
        networkRigidbody.enabled = false;
        networkAnimator.enabled = false;
        weaponManager.enabled = false;
        playerHealth.enabled = false;
        playerAiming.enabled = false;
        armature.SetActive(true);
        animator.enabled = false;
        weaponRifle.enabled = false;
        capsuleCollider.enabled = false;
        
        //  Revive the player after 5 seconds
        // Invoke(nameof(Revive), 5f);
    }

    public void Revive()
    {
        //  Enable every component that was disabled when the player died
        networkObject.enabled = true;
        networkTransform.enabled = true;
        surfCharacter.enabled = true;
        respawn.enabled = true;
        rb.isKinematic = false;
        networkRigidbody.enabled = true;
        networkAnimator.enabled = true;
        weaponManager.enabled = true;
        playerHealth.enabled = true;
        playerAiming.enabled = true;
        animator.enabled = true;
        armature.SetActive(false);
        weaponRifle.enabled = true;
        capsuleCollider.enabled = true;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
