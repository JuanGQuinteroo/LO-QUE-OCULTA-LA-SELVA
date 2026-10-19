using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class miguel_moviments : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D col;

    private float horizontal;
    private bool jumpRequested;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isCrouching;
    private bool isHiding = false;
    private bool isInHidingZone = false; // Detecta si está físicamente sobre los arbustos
    public bool IsHiding => isHiding; // Propiedad pública para que La Llorona pueda leerla
    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder; // Para recordar cuál era tu capa normal


    private bool lastCrouchingState = false;

    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4.5f;   // Velocidad al caminar
    [SerializeField] private float runSpeed = 8.5f;    // Velocidad al correr (con Shift)
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float crouchOffsetAdjustment = 0.2f; // <--- AÑADE ESTA LÍNEA AQUí

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Interacción")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayer;

    public bool IsCrouching => isCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder; // Guardamos el orden original (ej. 0)
        }

        if (col != null)
        {
            originalColliderSize = col.size;
            originalColliderOffset = col.offset;
        }
    }

    void Update()
    {
        horizontal = 0f;

        if (Keyboard.current != null)
        {
            // 1. Movimiento Horizontal
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal = 1f;
            }

            // 2. Agacharse
            isCrouching = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;

            // 3. Salto (solo si está en el suelo y no está agachado)
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isCrouching)
            {
                jumpRequested = true;
            }

            // 4. Interacción
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }

            // Presionar 'F' para ocultarse o salir del escondite
            if (isInHidingZone && Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            {
                isHiding = !isHiding;

                if (spriteRenderer != null)
                {
                    if (isHiding)
                    {
                        spriteRenderer.sortingOrder = -1; // Se pone detrás de los arbustos

                        // >>> CONGELA LA ALTURA Y (No sube ni baja por nada del mundo) <<<
                        rb.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
                        rb.linearVelocity = new Vector2(0f, 0f); // Detiene cualquier movimiento previo
                    }
                    else
                    {
                        spriteRenderer.sortingOrder = originalSortingOrder; // Regresa al frente

                        // >>> LIBERA LA ALTURA Y (Vuelve a aplicarle gravedad y movimiento) <<<
                        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    }
                }
            }

            // 5. Control de Animaciones (Parámetros exactos de tu Animator)
            if (animator != null)
            {
                bool isMoving = horizontal != 0f;
                bool isHoldingShift = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;

                // Está en el aire si NO está tocando suelo (y protegido con !isCrouching para evitar el bug de caída)
                animator.SetBool("jumping", !isGrounded && !isCrouching);

                // Corriendo
                bool isRunning = isMoving && isHoldingShift && !isCrouching;
                animator.SetBool("running", isRunning);

                // Caminando
                bool isWalking = isMoving && !isHoldingShift && !isCrouching;
                animator.SetBool("walking", isWalking);

                // Agachándose
                animator.SetBool("crouching", isCrouching && isGrounded);

                // Envía el estado de oculto al Animator
                animator.SetBool("isHiding", isHiding);
            }
        }

        // Voltear el sprite
        if (horizontal > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (horizontal < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        // Detección del suelo
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Decidir la velocidad actual
        bool isHoldingShift = Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
        float baseSpeed = isHoldingShift ? runSpeed : walkSpeed;
        float currentSpeed = isCrouching ? baseSpeed * crouchSpeedMultiplier : baseSpeed;

        // Aplicar movimiento horizontal
        rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);

        // Aplicar impulso de salto
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpRequested = false;
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si entra al área de los arbustos
        if (collision.gameObject.name == "Hidding" || collision.GetComponent<TilemapCollider2D>() != null)
        {
            isInHidingZone = true; // Ya está en la zona, listo para presionar 'F'
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Hidding" || collision.GetComponent<TilemapCollider2D>() != null)
        {
            isInHidingZone = false;
            isHiding = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder;
            }

            // Liberamos la posición Y al salir del escondite
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }
}