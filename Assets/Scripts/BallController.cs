using UnityEngine;

public class BallController : MonoBehaviour
{
    public float jumpForce = 5f;
    public Rigidbody rb;
    public bool isGrounded = false;

    public void ApplyInput(PlayerInput input)
    {
        if (input.jumpPressed && isGrounded)
        {
            rb.velocity = new Vector3(0, jumpForce, 0);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}
