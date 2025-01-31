using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [Header("Normal Player Controls")]
    public float movementSpeed;

    [Header("Focus Time Controls")]
    public float focusSpeed;
    public KeyCode focusKey = KeyCode.LeftShift;

    private GameObject circleChild;
    private SpriteRenderer circleRenderer;

    private bool isInFocusTime = false;
    private float currentMoveSpeed;

    private Rigidbody2D rb;
    private Vector2 input;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        circleChild = FindFirstObjectByType<SpriteRenderer>().gameObject;
        circleChild.transform.localScale = FloatToVec3(GetComponent<CircleCollider2D>().radius);
        circleRenderer = circleChild.GetComponent<SpriteRenderer>();
    }

    void Update() {
        // Handles the general movement
        HandleInput();

        // Handles Focus Time
        HandleFocusTime();
    }

    private void HandleInput() {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        input.Normalize();

        currentMoveSpeed = isInFocusTime ? focusSpeed : movementSpeed;
        rb.velocity = input * currentMoveSpeed;
    }

    private void HandleFocusTime() {
        if (Input.GetKeyDown(focusKey)) {
            isInFocusTime |= !isInFocusTime;
        }
        if (Input.GetKeyUp(focusKey) && isInFocusTime) {
            isInFocusTime &= !isInFocusTime;
        }

        circleRenderer.enabled = isInFocusTime;
    }

    public bool IsInFocusTime() { return isInFocusTime; }

    private Vector3 FloatToVec3(float x) { return new Vector3(x, x, x); }
}
