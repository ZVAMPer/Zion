using UnityEngine;

public class KnockMeter : MonoBehaviour
{
    public float knockMeter;
    public GameObject knockMeterUIPrefab;

    private void Start()
    {
        if (knockMeterUIPrefab != null)
        {
            GameObject uiInstance = Instantiate(knockMeterUIPrefab, transform);
            KnockMeterUI knockMeterUI = uiInstance.GetComponent<KnockMeterUI>();
            if (knockMeterUI != null)
            {
                knockMeterUI.knockMeter = this;
            }
        }
    }

    public void ApplyKnockback(Vector3 force, float knockMeterIncrement)
    {
        knockMeter += knockMeterIncrement;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }
}