using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Controla la caja de diálogo estilo novela visual: retrato, nombre, texto con efecto máquina de escribir.
// Debe haber UNO solo en la escena (se crea con Tools > Diálogo > Crear UI de diálogo en la escena).
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // Otros scripts pueden consultar esto (ej. para pausar la IA de La Llorona mientras se habla)
    public static bool IsDialogueActive => Instance != null && Instance.isActive;

    public static event Action OnDialogueStarted;
    public static event Action OnDialogueEnded;

    [Header("Componentes UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [Tooltip("Indicador de 'sigue' que aparece cuando termina de escribirse la línea")]
    [SerializeField] private GameObject continueIndicator;

    [Header("Ajustes")]
    [SerializeField] private float charsPerSecond = 40f;
    [Tooltip("Si está activo, Miguel no se puede mover mientras dura el diálogo")]
    [SerializeField] private bool freezePlayer = true;

    [Header("Referencias")]
    [SerializeField] private miguel_moviments playerMovement;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isActive;
    private bool isTyping;
    private int startFrame;
    private Coroutine typingCoroutine;
    private Action onFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<miguel_moviments>();
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        // Evita que la misma tecla que abrió el diálogo (ej. E) lo avance en el mismo frame
        if (Time.frameCount == startFrame) return;

        if (AdvancePressed())
        {
            if (isTyping)
            {
                // Primer toque: mostrar toda la línea de golpe
                CompleteTyping();
            }
            else
            {
                NextLine();
            }
        }
    }

    private bool AdvancePressed()
    {
        bool key = Keyboard.current != null &&
                   (Keyboard.current.eKey.wasPressedThisFrame ||
                    Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame);

        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        return key || click;
    }

    /// <summary>
    /// Inicia una conversación. onFinished se llama cuando se cierra la última línea.
    /// </summary>
    public void StartDialogue(DialogueData dialogue, Action onFinished = null)
    {
        if (isActive) return;
        if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        this.onFinished = onFinished;
        isActive = true;
        startFrame = Time.frameCount;

        if (freezePlayer) SetPlayerFrozen(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        OnDialogueStarted?.Invoke();
        ShowLine(currentDialogue.lines[currentLineIndex]);
    }

    private void ShowLine(DialogueLine line)
    {
        if (speakerNameText != null) speakerNameText.text = line.speakerName;

        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = line.portrait != null;
        }

        if (continueIndicator != null) continueIndicator.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        if (dialogueText != null)
        {
            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();

            int total = dialogueText.textInfo.characterCount;
            float visible = 0f;

            while (visible < total)
            {
                visible += charsPerSecond * Time.deltaTime;
                dialogueText.maxVisibleCharacters = Mathf.Min(Mathf.FloorToInt(visible), total);
                yield return null;
            }
        }

        FinishTyping();
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        FinishTyping();
    }

    private void FinishTyping()
    {
        isTyping = false;
        typingCoroutine = null;

        if (dialogueText != null) dialogueText.maxVisibleCharacters = int.MaxValue;
        if (continueIndicator != null) continueIndicator.SetActive(true);
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine(currentDialogue.lines[currentLineIndex]);
        }
    }

    private void EndDialogue()
    {
        isActive = false;
        isTyping = false;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = null;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (freezePlayer) SetPlayerFrozen(false);

        OnDialogueEnded?.Invoke();

        Action callback = onFinished;
        onFinished = null;
        callback?.Invoke();
    }

    private void SetPlayerFrozen(bool frozen)
    {
        if (playerMovement == null) return;

        if (frozen)
        {
            Rigidbody2D rb = playerMovement.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // Al desactivar el script, el Animator se queda con el último estado: lo ponemos en reposo
            Animator animator = playerMovement.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetBool("running", false);
                animator.SetBool("crouching", false);
            }
        }

        playerMovement.enabled = !frozen;
    }
}
