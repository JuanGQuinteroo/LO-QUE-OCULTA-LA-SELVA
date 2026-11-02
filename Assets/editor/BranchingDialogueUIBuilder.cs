using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

// Crea de un clic el Canvas con caja de diálogo + botones de opciones y el BranchingDialogueManager conectado.
// Menú: Tools > Diálogo > Crear UI de diálogo con opciones
// IMPORTANTE: este archivo debe estar dentro de una carpeta llamada "Editor".
public static class BranchingDialogueUIBuilder
{
    [MenuItem("Tools/Diálogo/Crear UI de diálogo con opciones")]
    private static void Build()
    {
        if (Object.FindAnyObjectByType<BranchingDialogueManager>() != null)
        {
            EditorUtility.DisplayDialog("Diálogo", "Ya hay un BranchingDialogueManager en esta escena.", "OK");
            return;
        }

        // Los botones necesitan un EventSystem para recibir clics
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Crear EventSystem");
        }

        GameObject canvasGO = new GameObject("BranchingDialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear UI de diálogo con opciones");

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 51;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel de fondo
        GameObject panel = CreateUI("DialoguePanel", canvasGO.transform, typeof(Image));
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
        SetRect(panel, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(100f, 40f), new Vector2(-100f, 340f));

        // Retrato
        GameObject portrait = CreateUI("Portrait", panel.transform, typeof(Image));
        Image portraitImage = portrait.GetComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.enabled = false;
        SetRect(portrait, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, -110f), new Vector2(250f, 110f));

        // Nombre
        GameObject nameGO = CreateUI("SpeakerName", panel.transform, typeof(TextMeshProUGUI));
        TextMeshProUGUI nameText = nameGO.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 40f;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = new Color(1f, 0.85f, 0.4f);
        nameText.alignment = TextAlignmentOptions.TopLeft;
        SetRect(nameGO, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(280f, -70f), new Vector2(-40f, -20f));

        // Texto
        GameObject textGO = CreateUI("DialogueText", panel.transform, typeof(TextMeshProUGUI));
        TextMeshProUGUI dialogueText = textGO.GetComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 36f;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        SetRect(textGO, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(280f, 40f), new Vector2(-40f, -80f));

        // Indicador de continuar
        GameObject continueGO = CreateUI("ContinueIndicator", panel.transform, typeof(TextMeshProUGUI));
        TextMeshProUGUI continueText = continueGO.GetComponent<TextMeshProUGUI>();
        continueText.text = ">>";
        continueText.fontSize = 36f;
        continueText.fontStyle = FontStyles.Bold;
        continueText.color = new Color(1f, 0.85f, 0.4f);
        continueText.alignment = TextAlignmentOptions.BottomRight;
        SetRect(continueGO, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140f, 15f), new Vector2(-25f, 70f));
        continueGO.SetActive(false);

        // Contenedor de opciones (encima del panel, a la derecha)
        GameObject choicesGO = CreateUI("ChoicesContainer", canvasGO.transform, typeof(VerticalLayoutGroup));
        VerticalLayoutGroup layout = choicesGO.GetComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.LowerRight;
        layout.spacing = 12f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        SetRect(choicesGO, new Vector2(0.4f, 0f), new Vector2(1f, 0f), new Vector2(0f, 360f), new Vector2(-100f, 760f));

        // Botón plantilla
        GameObject buttonGO = CreateUI("ChoiceButtonTemplate", choicesGO.transform, typeof(Image));
        Image buttonImage = buttonGO.GetComponent<Image>();
        buttonImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        Button button = buttonGO.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.85f, 0.4f);
        colors.pressedColor = new Color(0.8f, 0.65f, 0.2f);
        button.colors = colors;
        LayoutElement le = buttonGO.AddComponent<LayoutElement>();
        le.minHeight = 70f;
        le.preferredHeight = 70f;

        GameObject labelGO = CreateUI("Label", buttonGO.transform, typeof(TextMeshProUGUI));
        TextMeshProUGUI label = labelGO.GetComponent<TextMeshProUGUI>();
        label.fontSize = 32f;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        SetRect(labelGO, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(25f, 0f), new Vector2(-25f, 0f));

        buttonGO.SetActive(false);
        choicesGO.SetActive(false);

        // Manager conectado
        GameObject managerGO = new GameObject("BranchingDialogueManager", typeof(BranchingDialogueManager));
        Undo.RegisterCreatedObjectUndo(managerGO, "Crear BranchingDialogueManager");

        SerializedObject so = new SerializedObject(managerGO.GetComponent<BranchingDialogueManager>());
        so.FindProperty("dialoguePanel").objectReferenceValue = panel;
        so.FindProperty("speakerNameText").objectReferenceValue = nameText;
        so.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        so.FindProperty("portraitImage").objectReferenceValue = portraitImage;
        so.FindProperty("continueIndicator").objectReferenceValue = continueGO;
        so.FindProperty("choicesContainer").objectReferenceValue = choicesGO;
        so.FindProperty("choiceButtonTemplate").objectReferenceValue = button;
        so.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);

        EditorSceneManager.MarkSceneDirty(managerGO.scene);
        Selection.activeGameObject = managerGO;
    }

    private static GameObject CreateUI(string name, Transform parent, System.Type component)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), component);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void SetRect(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }
}
