using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // Evento estático: La UI o cualquier otro sistema se suscribe aquí
    // Pasa una ilustración opcional del mito que causó la muerte
    public static event Action<Sprite> OnPlayerDied;

    private static Vector3 lastCheckpointPosition;
    private static bool hasCheckpoint = false;

    private Rigidbody2D rb;
    private miguel_moviments movementScript;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<miguel_moviments>();

        // Si es el primer nivel y no hay checkpoint, la posición inicial es el punto por defecto
        if (!hasCheckpoint)
        {
            lastCheckpointPosition = transform.position;
            hasCheckpoint = true;
        }
    }

    /// <summary>
    /// Actualiza el punto de reaparición activo
    /// </summary>
    public static void SetCheckpoint(Vector3 newPosition)
    {
        lastCheckpointPosition = newPosition;
        hasCheckpoint = true;
        Debug.Log($"Checkpoint guardado en: {newPosition}");
    }

    /// <summary>
    /// Procesa la muerte instantánea (One-Hit KO)
    /// </summary>
    public void Die(Sprite mythIllustration = null)
    {
        // Desactivamos el control del personaje durante la muerte
        if (movementScript != null) movementScript.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Notificamos a la UI que el personaje murió
        OnPlayerDied?.Invoke(mythIllustration);
    }

    /// <summary>
    /// Reaparece a Miguel en la posición del último checkpoint
    /// </summary>
    public void Respawn()
    {
        transform.position = lastCheckpointPosition;

        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (movementScript != null) movementScript.enabled = true;
    }
}