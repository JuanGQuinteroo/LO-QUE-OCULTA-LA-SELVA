using UnityEngine;
using UnityEngine.InputSystem;

public class miguel_moviments : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator; // Referencia al Animator
    private float horizontal;
    private bool jumpRequested;
    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isCrouching;
    private CapsuleCollider2D col;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    [Header("Movimiento")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f; // 0 = quieto al agacharse, 1 = misma velocidad

    [Header("Detección de Suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Interacción")]
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayer;

    // Otros scripts (ej. la mecánica de La Llorona) podrán consultar esto para saber si Miguel está escondido
    public bool IsCrouching => isCrouching;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Obtenemos el componente Animator

        col = GetComponent<CapsuleCollider2D>();
        if (col != null)
        {
            // Guardamos las medidas iniciales para poder restaurarlas al levantarse
            originalColliderSize = col.size;
            originalColliderOffset = col.offset;
        }
    }

    void Update()
    {
        horizontal = 0f;

        if (Keyboard.current != null)
        {
            // Movimiento horizontal
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontal = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontal = 1f;
            }

            // Agacharse / esconderse
            isCrouching = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;

            // Ajustar el hitbox a la mitad al agacharse
            if (col != null)
            {
                if (isCrouching)
                {
                    // Reducir la altura a la mitad
                    col.size = new Vector2(originalColliderSize.x, originalColliderSize.y * 0.5f);
                    // Bajar el centro para mantener los pies pegados al piso
                    col.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - (originalColliderSize.y * 0.25f));
                }
                else
                {
                    // Restaurar tamaño y posición original del collider
                    col.size = originalColliderSize;
                    col.offset = originalColliderOffset;
                }
            }

            // Capturamos el salto solo si está en el suelo y no está agachado
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isCrouching)
            {
                jumpRequested = true;
            }

            // Interactuar (hablar con la Ranita, abrir objetos, etc.)
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract();
            }
        }

        // Actualizar las animaciones según el estado
        if (animator != null)
        {
            animator.SetBool("running", horizontal != 0f && !isCrouching);
            animator.SetBool("crouching", isCrouching);
        }

        // Determinar si debemos girar el sprite
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
        // Verificar si está tocando el suelo
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // Movimiento Horizontal (más lento si está agachado)
        float currentSpeed = isCrouching ? speed * crouchSpeedMultiplier : speed;
        rb.linearVelocity = new Vector2(horizontal * currentSpeed, rb.linearVelocity.y);

        // Aplicar salto
        if (jumpRequested)
        {
            // Reseteamos la velocidad vertical antes de aplicar el impulso para un salto consistente
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
        // Visualizar el rango de detección del suelo en el editor
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Visualizar el rango de interacción
        if (interactPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactPoint.position, interactRadius);
        }
    }
}