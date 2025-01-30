using UnityEngine;

public class WeaponGravityGun : WeaponBase
{
    public float grabRange = 10f;
    public float throwForce = 1000f;
    public Transform holdPoint;
    private Rigidbody grabbedObject;
    private Collider grabbedObjectCollider;
    private Vector3 currentVelocity;

    void Start()
    {
        FindPlayerCollider();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (grabbedObject == null)
            {
                TryGrabObject();
            }
            else
            {
                ThrowObject();
            }
        }

        if (Input.GetMouseButtonDown(1) && grabbedObject != null)
        {
            ReleaseObject();
        }

        if (grabbedObject != null)
        {
            MoveObject();
        }
    }

    private void FindPlayerCollider()
    {
        // GameObject player = GameObject.FindGameObjectWithTag("Player");
        // if (player != null)
        // {
        //     playerCollider = player.GetComponentInChildren<Collider>();
        //     if (playerCollider == null)
        //     {
        //         Debug.LogError("Player GameObject found, but no Collider component attached in children.");
        //     }
        // }
        // else
        // {
        //     Debug.LogWarning("Player GameObject not found. Ensure the player is tagged as 'Player'.");
        // }
    }

    private Collider FindColliderInChildren(Transform parent)
    {
        Collider collider = parent.GetComponent<Collider>();
        if (collider != null)
        {
            return collider;
        }

        foreach (Transform child in parent)
        {
            collider = FindColliderInChildren(child);
            if (collider != null)
            {
                return collider;
            }
        }

        return null;
    }

    private void TryGrabObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, grabRange))
        {
            if (hit.collider.CompareTag("Enemy")) return; // Ignore enemies

            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                grabbedObject = rb;
                grabbedObjectCollider = rb.GetComponent<Collider>();
                grabbedObject.useGravity = false;
                grabbedObject.linearDamping = 10;
                grabbedObject.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                grabbedObject.constraints = RigidbodyConstraints.FreezeRotation;

                // Disable collision with everything
                if (grabbedObjectCollider != null)
                {
                    grabbedObjectCollider.enabled = false;
                }
            }
        }
    }

    private void MoveObject()
    {
        Vector3 targetPosition = holdPoint.position;
        grabbedObject.MovePosition(Vector3.SmoothDamp(grabbedObject.position, targetPosition, ref currentVelocity, 0.1f));
    }

    private void ReleaseObject()
    {
        grabbedObject.useGravity = true;
        grabbedObject.linearDamping = 1;
        grabbedObject.collisionDetectionMode = CollisionDetectionMode.Discrete;
        grabbedObject.constraints = RigidbodyConstraints.None;

        // Re-enable collision with everything
        if (grabbedObjectCollider != null)
        {
            grabbedObjectCollider.enabled = true;
        }

        // Apply launch force
        grabbedObject.AddForce(transform.forward * throwForce);

        grabbedObject = null;
        grabbedObjectCollider = null;
    }

    private void ThrowObject()
    {
        grabbedObject.useGravity = true;
        grabbedObject.linearDamping = 1;
        grabbedObject.collisionDetectionMode = CollisionDetectionMode.Discrete;
        grabbedObject.constraints = RigidbodyConstraints.None;

        // Re-enable collision with everything
        if (grabbedObjectCollider != null)
        {
            grabbedObjectCollider.enabled = true;
        }

        // Apply throw force
        grabbedObject.AddForce(transform.forward * throwForce);

        grabbedObject = null;
        grabbedObjectCollider = null;
    }

    public override void UseWeapon()
    {
        // This method can be left empty or used for additional functionality
    }
}