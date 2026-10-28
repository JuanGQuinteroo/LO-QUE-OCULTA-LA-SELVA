using System;
using UnityEngine;

// Una sola línea de la conversación (lo que se ve en pantalla de una vez)
[Serializable]
public class DialogueLine
{
    [Tooltip("Nombre que aparece arriba del texto (ej. 'Ranita', 'Abuela')")]
    public string speakerName;

    [Tooltip("Retrato del personaje que habla. Si está vacío, se oculta el retrato.")]
    public Sprite portrait;

    [TextArea(2, 5)]
    public string text;
}

// Una conversación completa. Se crea desde el menú: clic derecho en Project > Create > Dialogo > Conversacion
[CreateAssetMenu(fileName = "NuevaConversacion", menuName = "Dialogo/Conversacion")]
public class DialogueData : ScriptableObject
{
    public DialogueLine[] lines;
}
