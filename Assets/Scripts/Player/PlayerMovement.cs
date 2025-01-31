using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float movementSpeed;

    private Rigidbody2D rb;
    private Vector2 input;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        input.Normalize();

        rb.velocity = input * movementSpeed;
    }
}
