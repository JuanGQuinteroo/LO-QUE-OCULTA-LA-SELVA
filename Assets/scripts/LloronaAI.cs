using UnityEngine;

public class LloronaAI : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    [SerializeField] private float speed = 4f; // Velocidad base de referencia
    [SerializeField] private float detectionRadius = 6f;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private miguel_moviments playerMovement;
    [SerializeField] private Animator animator; // Referencia al Animator

    [Header("Activación por diálogo")]
    [Tooltip("Déjalo en false: La Llorona se activa al terminar 'dialogo_llorona1'")]
    [SerializeField] private bool startActivated = false;

    [Header("Detención por objeto")]
    [Tooltip("Capas de los objetos que detienen a La Llorona por completo")]
    [SerializeField] private LayerMask stopLayers;

    // Los estados posibles del "cerebro" de La Llorona
    private enum State { Statica, Caminando, Persiguiendo }
    private State currentState = State.Statica;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float stateTimer;
    private int wanderDirection = 1; // 1 para derecha, -1 para izquierda

    private bool isActivated;
    private bool isStopped;

    // Los demás scripts (muerte, tensión) consultan esto
    public bool IsActive => isActivated && !isStopped;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        isActivated = startActivated;
    }

    // >>> Llama a este método cuando termine "dialogo_llorona1" <<<
    public void Activate()
    {
        isActivated = true;
    }

    // Detención total (también se puede llamar desde otros scripts)
    public void StopCompletely()
    {
        isStopped = true;
        currentState = State.Statica;
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        UpdateAnimator();
    }

    private void Update()
    {
        if (!IsActive) return;
        if (playerTransform == null || playerMovement == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Te ve solo si: está en el radio, NO estás agachado y NO estás escondido
        bool playerIsVisible = (distanceToPlayer <= detectionRadius)
                               && !playerMovement.IsCrouching
                               && !playerMovement.IsHiding;

        if (playerIsVisible)
        {
            currentState = State.Persiguiendo;
        }
        else
        {
            // Si estaba persiguiendo y lo pierde de vista, vuelve a vagar
            if (currentState == State.Persiguiendo) stateTimer = 0f;
            HandleWanderBehavior();
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (!IsActive)
        {
            // Inactiva o detenida: sin movimiento horizontal (la gravedad sigue)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float targetVelocityX = 0f;

        if (currentState == State.Persiguiendo && playerTransform != null)
        {
            float chaseSpeed = speed + 1f;
            float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);

            targetVelocityX = directionX * chaseSpeed;
            FlipSprite(directionX);
        }
        else if (currentState == State.Caminando)
        {
            float walkSpeed = Mathf.Max(0.1f, speed - 3f);

            targetVelocityX = wanderDirection * walkSpeed;
            FlipSprite(wanderDirection);
        }

        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
    }

    private void HandleWanderBehavior()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            ChooseNewRandomState();
        }
    }

    private void ChooseNewRandomState()
    {
        int randomChoice = Random.Range(0, 2);

        if (randomChoice == 0)
        {
            currentState = State.Statica;
            stateTimer = Random.Range(2f, 5f);
        }
        else
        {
            currentState = State.Caminando;
            stateTimer = Random.Range(3f, 6f);
            wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        }
    }

    private void FlipSprite(float directionX)
    {
        if (spriteRenderer != null && directionX != 0)
        {
            spriteRenderer.flipX = directionX < 0;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("isWalk", currentState == State.Caminando);
        animator.SetBool("isRun", currentState == State.Persiguiendo);
    }

    // ---- Detección del objeto que la detiene (trigger o colisión) ----
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckStopObject(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckStopObject(collision.gameObject);
    }

    private void CheckStopObject(GameObject obj)
    {
        if (isStopped) return;
        if (((1 << obj.layer) & stopLayers.value) != 0)
        {
            StopCompletely();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
