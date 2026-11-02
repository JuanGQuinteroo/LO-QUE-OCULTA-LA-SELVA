using System;
using UnityEngine;
using UnityEngine.Events;

// Una opción que el jugador puede elegir
[Serializable]
public class BranchOption
{
    [Tooltip("Texto del botón (ej. 'Preguntar por el pueblo')")]
    public string text;

    [Tooltip("ID del nodo al que salta esta opción. Si está vacío, el diálogo termina.")]
    public string nextId;

    [Tooltip("Se dispara al elegir esta opción (ej. activar algo, o NPC > Disappear)")]
    public UnityEvent onChosen;
}

// Un "nodo" = una pantalla de diálogo: alguien habla y, opcionalmente, el jugador elige qué responder
[Serializable]
public class BranchNode
{
    [Tooltip("Nombre único de este nodo (ej. 'inicio', 'respuesta_pueblo'). El primero de la lista es donde empieza.")]
    public string id;

    public string speakerName;
    public Sprite portrait;

    [TextArea(2, 5)]
    public string text;

    [Tooltip("Si hay opciones, el jugador debe elegir una. Si está vacío, se avanza con E/clic.")]
    public BranchOption[] options;

    [Tooltip("Solo se usa si NO hay opciones: ID del siguiente nodo. Vacío = termina el diálogo.")]
    public string nextId;
}

// Crear desde el menú: clic derecho en Project > Create > Dialogo > Conversacion con opciones
[CreateAssetMenu(fileName = "NuevaConversacionConOpciones", menuName = "Dialogo/Conversacion con opciones")]
public class BranchingDialogueData : ScriptableObject
{
    [Tooltip("El primer nodo de la lista es el que se muestra al empezar")]
    public BranchNode[] nodes;
}
