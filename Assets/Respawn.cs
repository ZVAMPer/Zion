using UnityEngine;

public class Respawn : MonoBehaviour
{
    Vector3 position;
    public float floorY = -300;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        position = transform.position;
        position.y = position.y + 5;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < floorY)
        {
            transform.position = position;
        }
    }
}
