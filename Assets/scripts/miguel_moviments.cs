using UnityEngine;
using UnityEngine.InputSystem;

public class miguel_moviments : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D col;
    private SpriteRenderer spriteRenderer;

    private float horizontal;
    private bool jumpRequested;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isCrouching;
    private bool isHiding = false;
    private bool isInHidingZone = false; // Detecta si está en el área del arbusto

    public bool IsHiding => isHiding;
    public bool IsCrouching => isCrouching;

    private int originalSortingOrder;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float runSpeed = 8.5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Interacción")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }

    void Update()
    {
        horizontal = 0f;

        if (Keyboard.current != null)
        {
            // Solo procesamos movimiento si NO está escondido
            if (!isHiding)
            {
                // 1. Movimiento Horizontal
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    horizontal = -1f;
                else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    horizontal = 1f;

                // 2. Agacharse
                isCrouching = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;

                // 3. Salto
                if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isCrouching)
                {
                    jumpRequested = true;
                }

                // 4. Interacción
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    TryInteract();
                }
            }

            // 5. Presionar 'F' para alternar Escondite
            if (isInHidingZone && Keyboard.current.fKey.wasPressedThisFrame)
            {
                SetHidingState(!isHiding);
            }

            // 6. Animaciones
            UpdateAnimations();
        }

        // Volteo de sprite
        if (horizontal > 0 && !isFacingRight) Flip();
        else if (horizontal < 0 && isFacingRight) Flip();
    }

    private void SetHidingState(bool hide)
    {
        isHiding = hide;

        if (isHiding)
        {
            // 1. Lo volvemos Trigger (Intangible: La Llorona no puede empujarlo)
            if (col != null) col.isTrigger = true;

            // 2. Visual: Lo mandamos detrás del arbusto
            if (spriteRenderer != null) spriteRenderer.sortingOrder = -1;

            // 3. Congelamos TODO para que no caiga por gravedad ni se mueva
            rb.linearVelocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            // 1. Vuelve a ser sólido
            if (col != null) col.isTrigger = false;

            // 2. Visual: Regresa a su capa frontal original
            if (spriteRenderer != null) spriteRenderer.sortingOrder = originalSortingOrder;

            // 3. Descongelamos posición (mantiene FreezeRotation en Z)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void FixedUpdate()
    {
        if (isHiding) return; // Si está escondido, no calculamos físicas de movimiento

        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        bool isHoldingShift = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        float baseSpeed = isHoldingShift ? runSpeed : walkSpeed;
        float currentSpeed = isCrouching ? baseSpeed * crouchSpeedMultiplier : baseSpeed;

        rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isMoving = horizontal != 0f;
        bool isHoldingShift = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        animator.SetBool("jumping", !isGrounded && !isCrouching && !isHiding);
        animator.SetBool("running", isMoving && isHoldingShift && !isCrouching && !isHiding);
        animator.SetBool("walking", isMoving && !isHoldingShift && !isCrouching && !isHiding);
        animator.SetBool("crouching", isCrouching && isGrounded && !isHiding);
        animator.SetBool("isHiding", isHiding);
    }

    private void TryInteract()
    {
        if (interactPoint == null) return;
        Collider2D hit = Physics2D.OverlapCircle(interactPoint.position, interactRadius, interactableLayer);
        if (hit != null)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            interactable?.Interact();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    // DETECCIÓN DEL ARBUSTO
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detecta el arbusto por su nombre o Tag
        if (collision.gameObject.name == "Hidding" || collision.CompareTag("HidingSpot"))
        {
            isInHidingZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Hidding" || collision.CompareTag("HidingSpot"))
        {
            isInHidingZone = false;

            // Si por alguna razón sale del arbusto mientras estaba escondido, lo restauramos
            if (isHiding)
            {
                SetHidingState(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (interactPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactPoint.position, interactRadius);
        }
    }
}