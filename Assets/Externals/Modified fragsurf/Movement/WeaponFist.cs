using UnityEngine;

public class WeaponFist : WeaponBase
{
    public AnimationCurve knockbackCurve;
    // public float knockbackForce;
    // public float knockMeterIncrement;
    // public float range;
    public LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    public override void UseWeapon()
    {
        Debug.Log("Using weapon: " + weaponName);
        Debug.Log("Knock meter increment: " + knockMeterIncrement);
        Debug.Log("Knockback force: " + knockbackForce);
        Debug.Log("Range: " + range);

        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = transform.forward;

        // Draw the raycast in the Scene and Game view
        DrawRay(rayOrigin, rayDirection * range);

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, range))
        {
            KnockMeter targetKnockMeter = hit.collider.GetComponent<KnockMeter>();
            Vector3 knockbackDirection = (hit.point - transform.position).normalized;
            knockbackDirection.y = 1; // Add upward force

            if (targetKnockMeter != null)
            {
                float knockbackMultiplier = knockbackCurve.Evaluate(targetKnockMeter.knockMeter);
                Vector3 knockbackForceVector = knockbackDirection * knockbackForce * knockbackMultiplier;
                targetKnockMeter.ApplyKnockback(knockbackForceVector, knockMeterIncrement);
            }
            else
            {
                // Apply original knockback force if no KnockMeter is found
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(knockbackDirection * (knockbackForce), ForceMode.Impulse);
                }
            }
        }
    }

    private void DrawRay(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, start + end);
    }
}