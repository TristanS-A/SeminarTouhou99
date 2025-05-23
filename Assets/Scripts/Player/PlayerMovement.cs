using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [Header("Normal Player Controls")]
    public float movementSpeed;

    [Header("Focus Time Controls")]
    public float focusSpeed;
    public KeyCode focusKey = KeyCode.LeftShift;

    [SerializeField] private SpriteRenderer circleRenderer;

    private bool isInFocusTime = false;
    private float currentMoveSpeed;

    private Rigidbody2D rb;
    private Vector2 input;

    [Header("Sounds")]
    [SerializeField] private AudioClip focusSFX;

    private Animator animator;

    void Start() {
        rb = GetComponent<Rigidbody2D>();

        //for movment animations
        animator = GetComponent<Animator>();

        EventSystem.fireEvent(new GameStartEvent(gameObject));

        PlayerInfo.PlayerTime = Time.time;
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

        // Switches between two movement
        currentMoveSpeed = isInFocusTime ? focusSpeed : movementSpeed;
        rb.velocity = input * currentMoveSpeed;

        if (input.x < 0)
        {
            animator.SetBool("IsMovingLeft", true);
            animator.SetBool("IsMovingRight", false);
        }
        else if (input.x > 0)
        {
            animator.SetBool("IsMovingLeft", false);
            animator.SetBool("IsMovingRight", true);
        }
        else
        {
            animator.SetBool("IsMovingLeft", false);
            animator.SetBool("IsMovingRight", false);
        }
    }

    private void HandleFocusTime() {
        // Handles the switch between if player is holding down the focus key and sets the isInFocusTime bool
        if (Input.GetKeyDown(focusKey)) {
            SoundManager.Instance.PlaySFXClip(focusSFX, transform, 1f);
            isInFocusTime |= !isInFocusTime;
        }
        if (Input.GetKeyUp(focusKey) && isInFocusTime) {
            isInFocusTime &= !isInFocusTime;
        }

        // Enables the circle sprite depending if the player isInFocusTime
        circleRenderer.enabled = isInFocusTime;
    }

    public bool IsInFocusTime() { return isInFocusTime; }
}
