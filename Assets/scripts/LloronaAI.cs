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

    // Los estados posibles del "cerebro" de La Llorona
    private enum State { Statica, Caminando, Persiguiendo }
    private State currentState = State.Statica;

    private Rigidbody2D rb; // <--- Referencia al Rigidbody 2D (Físicas)
    private SpriteRenderer spriteRenderer;
    private float stateTimer;
    private int wanderDirection = 1; // 1 para derecha, -1 para izquierda

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // <--- Obtenemos el Rigidbody al iniciar
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (playerTransform == null || playerMovement == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Comprobamos si está dentro del radio y si el jugador NO está agachado (Sigilo)
        bool playerIsVisible = (distanceToPlayer <= detectionRadius) && !playerMovement.IsCrouching;

        if (playerIsVisible)
        {
            // SI TE VE: Cambia a persecución
            currentState = State.Persiguiendo;
        }
        else
        {
            // SI NO TE VE: Comportamiento aleatorio (Estatica o Caminando)
            HandleWanderBehavior();
        }

        // Actualizamos las animaciones según el estado
        UpdateAnimator();
    }

    // USAMOS FixedUpdate PARA EL MOVIMIENTO FÍSICO (IGUAL QUE MIGUEL)
    private void FixedUpdate()
    {
        if (rb == null) return;

        float targetVelocityX = 0f;

        if (currentState == State.Persiguiendo)
        {
            // Velocidad al correr = velocidad base + 1
            float chaseSpeed = speed + 1f;
            float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);

            targetVelocityX = directionX * chaseSpeed;
            FlipSprite(directionX);
        }
        else if (currentState == State.Caminando)
        {
            // Velocidad al caminar = velocidad base - 3
            float walkSpeed = Mathf.Max(0.1f, speed - 3f);

            targetVelocityX = wanderDirection * walkSpeed;
            FlipSprite(wanderDirection);
        }
        else
        {
            // Estática (se detiene en horizontal)
            targetVelocityX = 0f;
        }

        // Aplicamos la velocidad respetando la gravedad natural en Y (igual que Miguel)
        rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
    }

    private void HandleWanderBehavior()
    {
        stateTimer -= Time.deltaTime;

        // Si el temporizador llega a cero, elige un nuevo estado al azar
        if (stateTimer <= 0f)
        {
            ChooseNewRandomState();
        }
    }

    private void ChooseNewRandomState()
    {
        // 0 = Estática, 1 = Caminando
        int randomChoice = Random.Range(0, 2);

        if (randomChoice == 0)
        {
            currentState = State.Statica;
            stateTimer = Random.Range(2f, 5f); // Cuánto tiempo se queda quieta
        }
        else
        {
            currentState = State.Caminando;
            stateTimer = Random.Range(3f, 6f); // Cuánto tiempo camina
            wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1; // Dirección aleatoria
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

        // Traducimos el estado actual a los parámetros booleanos de tu Animator
        bool isWalking = (currentState == State.Caminando);
        bool isRunning = (currentState == State.Persiguiendo);

        animator.SetBool("isWalk", isWalking);
        animator.SetBool("isRun", isRunning);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar el rango de detección en color magenta en el editor
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}