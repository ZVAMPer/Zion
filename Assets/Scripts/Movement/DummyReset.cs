using UnityEngine;

public class DummyReset : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private KnockMeter knockMeter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        knockMeter = GetComponent<KnockMeter>();
    }

    // Update is called once per frame
    void Update()
    {
        // Reset the dummy's position and knock meter when the 'R' key is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetDummy();
        }
    }

    private void ResetDummy()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (knockMeter != null)
        {
            knockMeter.knockMeter = 0;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}