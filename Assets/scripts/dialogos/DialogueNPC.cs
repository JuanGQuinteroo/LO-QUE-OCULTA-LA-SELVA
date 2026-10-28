using UnityEngine;
using UnityEngine.InputSystem;

// Pon este script en el nuevo personaje. Cuando Miguel se acerca a "triggerDistance",
// se abre el diálogo (automáticamente o al presionar E, según lo configures).
public class DialogueNPC : MonoBehaviour
{
    [Header("Conversación")]
    [SerializeField] private DialogueData dialogue;

    [Header("Activación por distancia")]
    [SerializeField] private float triggerDistance = 4f;
    [Tooltip("Si está activo, el diálogo solo se muestra una vez")]
    [SerializeField] private bool triggerOnce = true;
    [Tooltip("Si está activo, hay que presionar E estando cerca. Si no, el diálogo salta solo al acercarse.")]
    [SerializeField] private bool requireInteractKey = false;

    [Header("Opcionales")]
    [Tooltip("Objeto que se muestra cuando se puede hablar (ej. un letrero 'E'). Solo se usa con requireInteractKey.")]
    [SerializeField] private GameObject interactPrompt;
    [Tooltip("Gira el sprite hacia Miguel al empezar a hablar (asume que el sprite original mira a la derecha)")]
    [SerializeField] private bool facePlayerWhenTalking = false;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;

    private bool hasPlayed = false;
    private bool armed = true; // Se vuelve a armar cuando Miguel se aleja
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            miguel_moviments player = FindAnyObjectByType<miguel_moviments>();
            if (player != null) playerTransform = player.transform;
        }

        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerTransform == null || dialogue == null) return;

        // Mientras se está hablando (con este u otro NPC) no hacemos nada
        if (DialogueManager.IsDialogueActive)
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= triggerDistance;
        bool exhausted = hasPlayed && triggerOnce;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(inRange && requireInteractKey && !exhausted);
        }

        if (!inRange)
        {
            armed = true;
            return;
        }

        if (!armed || exhausted) return;

        if (requireInteractKey)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                BeginDialogue();
            }
        }
        else
        {
            BeginDialogue();
        }
    }

    private void BeginDialogue()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("No hay un DialogueManager en la escena. Usa Tools > Diálogo > Crear UI de diálogo en la escena.");
            return;
        }

        armed = false;
        hasPlayed = true;

        if (facePlayerWhenTalking && spriteRenderer != null)
        {
            spriteRenderer.flipX = playerTransform.position.x < transform.position.x;
        }

        if (interactPrompt != null) interactPrompt.SetActive(false);

        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar el rango de activación en el editor de Unity
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
