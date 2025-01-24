using UnityEngine;

public class WeaponAirgun : WeaponBase
{
    // public float knockbackForce;
    // public float knockMeterIncrement;
    public AnimationCurve knockbackCurve;
    public float coneAngle = 45f;
    public float coneRange = 10f;
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

        Collider[] hits = Physics.OverlapSphere(transform.position, coneRange);
        foreach (Collider hit in hits)
        {
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget <= coneAngle / 2)
            {
                KnockMeter targetKnockMeter = hit.GetComponent<KnockMeter>();
                Vector3 knockbackDirection = directionToTarget;
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
                    Rigidbody rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                    }
                }
            }
        }

        // Draw the cone in the Game view
        DrawCone();
    }

    private void DrawCone()
    {
        Vector3 forward = transform.forward * coneRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -coneAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, coneAngle / 2, 0) * forward;

        int segments = 20;
        lineRenderer.positionCount = segments + 3;
        lineRenderer.SetPosition(0, transform.position);

        for (int i = 0; i <= segments; i++)
        {
            float angle = -coneAngle / 2 + (coneAngle / segments) * i;
            Vector3 segmentDirection = Quaternion.Euler(0, angle, 0) * forward;
            lineRenderer.SetPosition(i + 1, transform.position + segmentDirection);
        }

        lineRenderer.SetPosition(segments + 1, transform.position + rightBoundary);
        lineRenderer.SetPosition(segments + 2, transform.position); // Close the cone
    }
}