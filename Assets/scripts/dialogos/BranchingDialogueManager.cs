using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

// Caja de diálogo con opciones y respuestas distintas según lo que elija el jugador.
// Es independiente de DialogueManager. Debe haber UNO solo en la escena
// (Tools > Diálogo > Crear UI de diálogo con opciones).
public class BranchingDialogueManager : MonoBehaviour
{
    public static BranchingDialogueManager Instance { get; private set; }
    public static bool IsActive => Instance != null && Instance.isActive;

    public static event Action OnStarted;
    public static event Action OnEnded;

    [Header("Componentes UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject continueIndicator;

    [Header("Opciones")]
    [SerializeField] private GameObject choicesContainer;
    [Tooltip("Botón plantilla (desactivado) con un TMP_Text hijo. Se clona por cada opción.")]
    [SerializeField] private Button choiceButtonTemplate;

    [Header("Ajustes")]
    [SerializeField] private float charsPerSecond = 40f;
    [SerializeField] private bool freezePlayer = true;

    [Header("Referencias")]
    [SerializeField] private miguel_moviments playerMovement;

    private BranchingDialogueData currentData;
    private BranchNode currentNode;
    private readonly Dictionary<string, BranchNode> nodeById = new Dictionary<string, BranchNode>();
    private readonly List<GameObject> spawnedChoices = new List<GameObject>();

    private bool isActive;
    private bool isTyping;
    private bool showingChoices;
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
        if (playerMovement == null) playerMovement = FindAnyObjectByType<miguel_moviments>();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choiceButtonTemplate != null) choiceButtonTemplate.gameObject.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;
        if (Time.frameCount == startFrame) return;

        if (showingChoices)
        {
            HandleChoiceHotkeys();
            return;
        }

        if (AdvancePressed())
        {
            if (isTyping) CompleteTyping();
            else GoTo(currentNode.nextId);
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

    // Teclas 1..9 para elegir sin mouse
    private void HandleChoiceHotkeys()
    {
        if (Keyboard.current == null || currentNode.options == null) return;

        int max = Mathf.Min(currentNode.options.Length, 9);
        for (int i = 0; i < max; i++)
        {
            Key key = (Key)((int)Key.Digit1 + i);
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                SelectOption(currentNode.options[i]);
                return;
            }
        }
    }

    public void StartDialogue(BranchingDialogueData data, Action onFinished = null)
    {
        if (isActive || DialogueManager.IsDialogueActive) return;
        if (data == null || data.nodes == null || data.nodes.Length == 0) return;

        currentData = data;
        nodeById.Clear();
        foreach (BranchNode n in data.nodes)
        {
            if (!string.IsNullOrEmpty(n.id)) nodeById[n.id] = n;
        }

        this.onFinished = onFinished;
        isActive = true;
        startFrame = Time.frameCount;

        if (freezePlayer) SetPlayerFrozen(true);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        OnStarted?.Invoke();
        ShowNode(data.nodes[0]);
    }

    private void GoTo(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            EndDialogue();
            return;
        }

        if (!nodeById.TryGetValue(id, out BranchNode node))
        {
            Debug.LogWarning($"[BranchingDialogue] No existe el nodo '{id}' en {currentData.name}. Se termina el diálogo.");
            EndDialogue();
            return;
        }

        ShowNode(node);
    }

    private void ShowNode(BranchNode node)
    {
        currentNode = node;
        ClearChoices();

        if (speakerNameText != null) speakerNameText.text = node.speakerName;

        if (portraitImage != null)
        {
            portraitImage.sprite = node.portrait;
            portraitImage.enabled = node.portrait != null;
        }

        if (continueIndicator != null) continueIndicator.SetActive(false);

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(node.text));
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

        bool hasOptions = currentNode.options != null && currentNode.options.Length > 0
                          && choiceButtonTemplate != null && choicesContainer != null;

        if (hasOptions) ShowChoices(currentNode.options);
        else if (continueIndicator != null) continueIndicator.SetActive(true);
    }

    private void ShowChoices(BranchOption[] options)
    {
        ClearChoices();
        showingChoices = true;
        choicesContainer.SetActive(true);

        for (int i = 0; i < options.Length; i++)
        {
            BranchOption option = options[i];

            Button button = Instantiate(choiceButtonTemplate, choicesContainer.transform);
            button.gameObject.SetActive(true);

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = (i + 1) + ". " + option.text;

            button.onClick.AddListener(() => SelectOption(option));
            spawnedChoices.Add(button.gameObject);
        }
    }

    private void ClearChoices()
    {
        showingChoices = false;

        foreach (GameObject go in spawnedChoices)
        {
            if (go != null) Destroy(go);
        }
        spawnedChoices.Clear();

        if (choicesContainer != null) choicesContainer.SetActive(false);
    }

    private void SelectOption(BranchOption option)
    {
        if (!isActive || !showingChoices) return;

        ClearChoices();
        option.onChosen?.Invoke();

        if (!isActive) return;
        GoTo(option.nextId);
    }

    private void EndDialogue()
    {
        isActive = false;
        isTyping = false;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = null;

        ClearChoices();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (freezePlayer) SetPlayerFrozen(false);

        OnEnded?.Invoke();

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
