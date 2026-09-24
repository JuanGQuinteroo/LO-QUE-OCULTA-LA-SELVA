using UnityEngine;

public class LloronaAI : MonoBehaviour
{
    [Header("Configuración de Persecución")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float detectionRadius = 6f;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private miguel_moviments playerMovement;

    private bool isChasing = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (playerTransform == null || playerMovement == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // Si el jugador entra en el radio de visión
        if (distanceToPlayer <= detectionRadius)
        {
            // Mecánica de Sigilo: Si Miguel NO está agachado, La Llorona lo detecta y lo persigue
            if (!playerMovement.IsCrouching)
            {
                isChasing = true;
            }
            else
            {
                // Si Miguel se agacha a tiempo, La Llorona lo pierde de vista
                isChasing = false;
            }
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        // Mover hacia la posición del jugador
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Girar el sprite de La Llorona según la dirección
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar el rango de detección en el editor de Unity
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}