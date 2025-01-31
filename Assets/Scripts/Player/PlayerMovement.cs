using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    public float movementSpeed;

    private Rigidbody2D rb;
    private Vector2 input;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        input.Normalize();

        rb.velocity = input * movementSpeed;
    }
}
