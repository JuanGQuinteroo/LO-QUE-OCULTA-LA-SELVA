using System.Collections;
using UnityEngine;

public class LloronaAI : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float detectionRadius = 6f;

    [Header("Escape y Muerte")]
    [Tooltip("Multiplicador del radio para que muera (ej. 2 = al doble de distancia)")]
    [SerializeField] private float escapeDistanceMultiplier = 2f;
    [Tooltip("Arrastra aquí el GameObject de la siguiente Llorona en la escena (opcional)")]
    [SerializeField] private GameObject nextLlorona;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private miguel_moviments playerMovement;
    [SerializeField] private Animator animator;

    [Header("Activación por diálogo")]
    [SerializeField] private bool startActivated = false;

    [Header("Detención por objeto")]
    [SerializeField] private LayerMask stopLayers;

    private enum State { Statica, Caminando, Persiguiendo }
    private State currentState = State.Statica;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float stateTimer;
    private int wanderDirection = 1;

    private bool isActivated;
    private bool isStopped;
    private bool isDead = false;              // Control de muerte
    private bool hasChasedPlayer = false;      // Solo morirá si ya inició una persecución previa

    private int originalSortingOrder;

    // Los demás scripts (tensión, etc.) sabrán si está viva y activa
    public bool IsActive => isActivated && !isStopped && !isDead;

    // Devuelve 'true' EXACTAMENTE cuando te está persiguiendo activamente
    public bool IsChasing => currentState == State.Persiguiendo && IsActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        isActivated = startActivated;

        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }

    public void Activate()
    {
        isActivated = true;
    }

    public void StopCompletely()
    {
        if (isDead) return;
        isStopped = true;
        currentState = State.Statica;
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        UpdateAnimator();
    }

    private void Update()
    {
        if (!IsActive) return;
        if (playerTransform == null || playerMovement == null) return;

        // Capas visuales al esconderse Miguel
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = playerMovement.IsHiding ? 2 : originalSortingOrder;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Radio de detección y visibilidad
        bool playerIsVisible = (distanceToPlayer <= detectionRadius)
                               && !playerMovement.IsCrouching
                               && !playerMovement.IsHiding;

        if (playerIsVisible)
        {
            currentState = State.Persiguiendo;
            hasChasedPlayer = true; // Confirmamos que ya empezó a cazar a Miguel
        }
        else
        {
            if (currentState == State.Persiguiendo) stateTimer = 0f;
            HandleWanderBehavior();
        }

        // >>> CONDICIÓN DE MUERTE: Si ya empezó la cacería y Miguel escapó al doble del radio <<<
        float deathDistance = detectionRadius * escapeDistanceMultiplier;
        if (hasChasedPlayer && distanceToPlayer >= deathDistance)
        {
            Die();
            return;
        }

        UpdateAnimator();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. Detener físicas por completo
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 2. Apagar animaciones de movimiento
        if (animator != null)
        {
            animator.SetBool("isWalk", false);
            animator.SetBool("isRun", false);
        }

        // 3. Activar y despertar a la siguiente Llorona del nivel (si la configuraste)
        if (nextLlorona != null)
        {
            nextLlorona.SetActive(true);
            LloronaAI nextAI = nextLlorona.GetComponent<LloronaAI>();
            if (nextAI != null)
            {
                nextAI.Activate();
            }
        }

        // 4. Iniciar desvanecimiento fantasmal y destrucción
        StartCoroutine(FadeOutAndDestroyRoutine());
    }

    private IEnumerator FadeOutAndDestroyRoutine()
    {
        // Apagamos sus colisionadores para que ya no interactúe con nada
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var c in colliders) c.enabled = false;

        float duration = 1.2f; // Tiempo que tarda en disolverse (segundos)
        float elapsed = 0f;
        Color initialColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (spriteRenderer != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                spriteRenderer.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            }
            yield return null;
        }

        // Destruir este GameObject para liberar memoria y dar cupo limpio
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (!IsActive || rb == null) return;

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
        if (isStopped || isDead) return;
        if (((1 << obj.layer) & stopLayers.value) != 0)
        {
            StopCompletely();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Círculo Magenta: Radio de visión
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Círculo Rojo: Radio de muerte por escape (el doble)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius * escapeDistanceMultiplier);
    }
}