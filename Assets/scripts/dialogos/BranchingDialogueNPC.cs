using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// NPC con el que se interactúa (por defecto con E) y que ofrece opciones de respuesta.
// Funciona con BranchingDialogueManager. Puede desaparecer al terminar o cuando una opción lo pida.
public class BranchingDialogueNPC : MonoBehaviour
{
    [Header("Conversación")]
    [SerializeField] private BranchingDialogueData dialogue;

    [Header("Al terminar el diálogo")]
    [SerializeField] private UnityEvent onDialogueFinished;

    [Header("Desaparecer")]
    [Tooltip("Desaparece automáticamente cuando termina el diálogo. También puedes llamar a Disappear() desde una opción (On Chosen).")]
    [SerializeField] private bool disappearAfterDialogue = false;
    [SerializeField] private float disappearDelay = 0f;
    [Tooltip("Segundos de desvanecimiento (0 = desaparece de golpe)")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool destroyInsteadOfDeactivate = false;

    [Header("Interacción")]
    [SerializeField] private float triggerDistance = 3f;
    [Tooltip("Si está activo, hay que presionar E estando cerca. Si no, el diálogo salta solo al acercarse.")]
    [SerializeField] private bool requireInteractKey = true;
    [Tooltip("Si está activo, solo se puede hablar una vez")]
    [SerializeField] private bool talkOnce = false;
    [Tooltip("Letrero opcional (ej. 'E') que se muestra cuando se puede hablar")]
    [SerializeField] private GameObject interactPrompt;
    [Tooltip("Gira el sprite hacia Miguel al hablar (asume que el sprite original mira a la derecha)")]
    [SerializeField] private bool facePlayerWhenTalking = false;

    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;

    private bool hasTalked;
    private bool armed = true;
    private bool disappearing;
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
        if (disappearing || playerTransform == null || dialogue == null) return;

        if (BranchingDialogueManager.IsActive || DialogueManager.IsDialogueActive)
        {
            if (interactPrompt != null) interactPrompt.SetActive(false);
            return;
        }

        bool inRange = Vector2.Distance(transform.position, playerTransform.position) <= triggerDistance;
        bool exhausted = talkOnce && hasTalked;

        if (interactPrompt != null)
            interactPrompt.SetActive(inRange && requireInteractKey && !exhausted);

        if (!inRange)
        {
            armed = true;
            return;
        }

        if (exhausted) return;

        if (requireInteractKey)
        {
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) BeginDialogue();
        }
        else if (armed)
        {
            BeginDialogue();
        }
    }

    private void BeginDialogue()
    {
        if (BranchingDialogueManager.Instance == null)
        {
            Debug.LogWarning("No hay un BranchingDialogueManager en la escena. Usa Tools > Diálogo > Crear UI de diálogo con opciones.");
            return;
        }

        armed = false;
        hasTalked = true;

        if (facePlayerWhenTalking && spriteRenderer != null)
            spriteRenderer.flipX = playerTransform.position.x < transform.position.x;

        if (interactPrompt != null) interactPrompt.SetActive(false);

        BranchingDialogueManager.Instance.StartDialogue(dialogue, OnDialogueDone);
    }

    private void OnDialogueDone()
    {
        onDialogueFinished?.Invoke();
        if (disappearAfterDialogue) Disappear();
    }

    /// <summary>Hace desaparecer al NPC. Se puede llamar desde un UnityEvent (ej. On Chosen de una opción).</summary>
    public void Disappear()
    {
        if (disappearing) return;
        disappearing = true;

        if (interactPrompt != null) interactPrompt.SetActive(false);

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        if (disappearDelay > 0f) yield return new WaitForSeconds(disappearDelay);

        if (fadeDuration > 0f)
        {
            SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
            Color[] startColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) startColors[i] = renderers[i].color;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(t / fadeDuration);

                for (int i = 0; i < renderers.Length; i++)
                {
                    Color c = startColors[i];
                    c.a = startColors[i].a * alpha;
                    renderers[i].color = c;
                }
                yield return null;
            }
        }

        if (destroyInsteadOfDeactivate) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
