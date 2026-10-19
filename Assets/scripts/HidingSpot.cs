using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    [Header("Aviso Visual de este objeto")]
    [SerializeField] private GameObject promptUI; // El texto "[F]" específico de este arbusto

    private void Start()
    {
        // Nos aseguramos de que el letrero empiece apagado
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si Miguel entra en la zona de este arbusto específico
        if (collision.GetComponent<miguel_moviments>() != null)
        {
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Si Miguel sale de la zona de este arbusto
        if (collision.GetComponent<miguel_moviments>() != null)
        {
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}