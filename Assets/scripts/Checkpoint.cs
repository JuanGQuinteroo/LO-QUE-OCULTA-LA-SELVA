using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform respawnTransform;
    [SerializeField] private bool activateOnTrigger = false;

    private void Awake()
    {
        if (respawnTransform == null)
        {
            respawnTransform = transform;
        }
    }

    // Guardado por interacción (Teclas E / La Ranita)
    public void Interact()
    {
        RegisterCheckpoint();
    }

    // Guardado automático por contacto al pasar por la zona
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activateOnTrigger && collision.CompareTag("Player"))
        {
            RegisterCheckpoint();
        }
    }

    private void RegisterCheckpoint()
    {
        PlayerHealth.SetCheckpoint(respawnTransform.position);
    }
}