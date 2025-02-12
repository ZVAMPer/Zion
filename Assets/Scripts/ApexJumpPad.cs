using Fragsurf.Movement;
using UnityEngine;

public class ApexJumpPad : MonoBehaviour
{
    public float forwardBoost = 10f;
    public float upwardBoost = 10f;
    public float gravityMultiplier = 0.5f;
    public float duration = 1f;

    private void OnTriggerEnter(Collider other)
    {
        SurfCharacter character = other.GetComponentInParent<SurfCharacter>();
        if (character != null && character.IsOwner)
        {
            // 1) Get the player's current velocity
            Vector3 currentVelocity = character.moveData.velocity;

            // 2) Zero out the vertical component so we ignore downward speed
            currentVelocity.y = 0f;

            // 3) Determine the launch direction based on current horizontal velocity
            Vector3 launchDir = currentVelocity.magnitude > 0.1f
                ? currentVelocity.normalized
                : character.transform.forward;

            // 4) Construct the final jump velocity
            //    - We keep the player's horizontal momentum (currentVelocity),
            //      add forwardBoost in the launchDir, and add an upwardBoost
            Vector3 jumpVelocity = currentVelocity
                                  + (launchDir * forwardBoost)
                                  + (Vector3.up * upwardBoost);

            // 5) Apply the Apex Jump
            character.ApplyApexJump(jumpVelocity, gravityMultiplier, duration);
        }
    }
}
