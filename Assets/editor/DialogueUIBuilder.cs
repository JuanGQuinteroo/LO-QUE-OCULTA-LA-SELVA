using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Crea de un clic el Canvas con la caja de diálogo y el DialogueManager ya conectado.
// Menú: Tools > Diálogo > Crear UI de diálogo en la escena
public static class DialogueUIBuilder
{
    [MenuItem("Tools/Diálogo/Crear UI de diálogo en la escena")]
    private static void BuildDialogueUI()
    {
        if (Object.FindAnyObjectByType<DialogueManager>() != null)
        {
            EditorUtility.DisplayDialog("Diálogo", "Ya hay un DialogueManager en esta escena.", "OK");
            return;
        }

        // Canvas raíz
        GameObject canvasGO = new GameObject("DialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear UI de diálogo");

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Panel de fondo (parte baja de la pantalla)
        GameObject panel = CreateUI("DialoguePanel", canvasGO.transform, typeof(Image));
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
        SetRect(panel, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(100f, 40f), new Vector2(-100f, 340f));

        // Retrato (izquierda)
        GameObject portrait = CreateUI("Portrait", panel.transform, typeof(Image));
        Image portraitImage = portrait.GetComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.enabled = false;
        SetRect(portrait, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, -110f), new Vector2(250f, 110f));

        // Nombre del que habla
        GameObject nameGO = CreateUI("SpeakerName", panel.transform, typeof(TextMeshProUGUI));
        TextMeshProUGUI nameText = nameGO.GetComponent<TextMeshProUGUI>();
        nameText.fontSize = 40f;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = new Color(1f, 0.85f, 0.4f);
        nameText.alignment = TextAlignmentOptions.TopLeft;
        SetRect(nameGO, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(280f, -70f), new Vector2(-40f, -20f));

        // Texto del diálogo
        GameObject textGO = CreateUI("DialogueText", panel.transform, typeof(TextMeshProUGUI));
        TextMeshProUGUI dialogueText = textGO.GetComponent<TextMeshProUGUI>();
        dialogueText.fontSize = 36f;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        SetRect(textGO, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(280f, 40f), new Vector2(-40f, -80f));

        // Indicador de "continuar" (abajo a la derecha)
        GameObject continueGO = CreateUI("ContinueIndicator", panel.transform, typeof(TextMeshProUGUI));
        TextMeshProUGUI continueText = continueGO.GetComponent<TextMeshProUGUI>();
        continueText.text = ">>";
        continueText.fontSize = 36f;
        continueText.fontStyle = FontStyles.Bold;
        continueText.color = new Color(1f, 0.85f, 0.4f);
        continueText.alignment = TextAlignmentOptions.BottomRight;
        SetRect(continueGO, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-140f, 15f), new Vector2(-25f, 70f));
        continueGO.SetActive(false);

        // DialogueManager con todas las referencias conectadas
        GameObject managerGO = new GameObject("DialogueManager", typeof(DialogueManager));
        Undo.RegisterCreatedObjectUndo(managerGO, "Crear DialogueManager");

        SerializedObject so = new SerializedObject(managerGO.GetComponent<DialogueManager>());
        so.FindProperty("dialoguePanel").objectReferenceValue = panel;
        so.FindProperty("speakerNameText").objectReferenceValue = nameText;
        so.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        so.FindProperty("portraitImage").objectReferenceValue = portraitImage;
        so.FindProperty("continueIndicator").objectReferenceValue = continueGO;
        so.ApplyModifiedPropertiesWithoutUndo();

        // El panel arranca oculto; el DialogueManager lo muestra cuando empieza una conversación
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
