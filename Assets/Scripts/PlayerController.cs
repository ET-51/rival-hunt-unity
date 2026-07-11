using UnityEngine;

/// Reads movement input and moves the player. Nothing else lives here.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Read input in Update so no key presses are missed.
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized; // diagonal movement shouldn't be faster
    }

    private void FixedUpdate()
    {
        // Apply movement in FixedUpdate because physics runs there.
        rb.linearVelocity = input * moveSpeed;
    }
}
