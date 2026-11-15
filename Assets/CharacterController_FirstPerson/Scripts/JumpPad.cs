using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 15f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FP_CharacterController player))
        {
            player.ApplyJumpImpulse(jumpForce);
        }
    }
}