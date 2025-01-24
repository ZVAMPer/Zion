using UnityEngine;
using TMPro;

public class KnockMeterUI : MonoBehaviour
{
    public KnockMeter knockMeter;
    public TMP_Text knockMeterText;
    public Vector3 offset;

    void Start()
    {
        if (knockMeter == null)
        {
            knockMeter = GetComponentInParent<KnockMeter>();
            if (knockMeter == null)
            {
                Debug.LogError("KnockMeter component not found in parent.");
            }
            else
            {
                Debug.Log("KnockMeter component found and assigned.");
            }
        }

        if (knockMeterText == null)
        {
            knockMeterText = GetComponentInChildren<TMP_Text>();
            if (knockMeterText == null)
            {
                Debug.LogError("TMP_Text component not found in children.");
            }
            else
            {
                Debug.Log("TMP_Text component found and assigned.");
            }
        }
    }

    void Update()
    {
        if (knockMeter != null && knockMeterText != null)
        {
            knockMeterText.text = knockMeter.knockMeter.ToString("F1");
            Vector3 worldPosition = knockMeter.transform.position + offset;
            transform.position = worldPosition;
        }
        else
        {
            if (knockMeter == null)
            {
                Debug.LogError("knockMeter is null in Update.");
            }
            if (knockMeterText == null)
            {
                Debug.LogError("knockMeterText is null in Update.");
            }
        }
    }
}