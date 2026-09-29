using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("Fase 1: Destello con la ilustración del mito")]
    [SerializeField] private GameObject illustrationPanel;
    [SerializeField] private Image mythImageDisplay;
    [SerializeField] private float illustrationDuration = 1.5f;

    [Header("Fase 2: Menú Reintentar / Salir")]
    [SerializeField] private GameObject retryMenuPanel;

    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealthReference;

    private void OnEnable()
    {
        // Nos suscribimos al evento estático cuando este objeto se activa
        PlayerHealth.OnPlayerDied += HandlePlayerDied;
    }

    private void OnDisable()
    {
        // Cancelamos la suscripción para evitar fugas de memoria (Memory Leaks)
        PlayerHealth.OnPlayerDied -= HandlePlayerDied;
    }

    private void Start()
    {
        if (illustrationPanel != null) illustrationPanel.SetActive(false);
        if (retryMenuPanel != null) retryMenuPanel.SetActive(false);
    }

    private void HandlePlayerDied(Sprite mythIllustration)
    {
        StartCoroutine(DeathSequence(mythIllustration));
    }

    // Corrutina: primero muestra el destello con la ilustración, espera, y luego muestra el menú
    private IEnumerator DeathSequence(Sprite mythIllustration)
    {
        if (mythImageDisplay != null)
        {
            mythImageDisplay.sprite = mythIllustration;
            mythImageDisplay.enabled = mythIllustration != null;
        }
        if (illustrationPanel != null) illustrationPanel.SetActive(true);

        yield return new WaitForSeconds(illustrationDuration);

        if (illustrationPanel != null) illustrationPanel.SetActive(false);
        if (retryMenuPanel != null) retryMenuPanel.SetActive(true);
    }

    // Método asignado al botón "Reintentar" del Canvas UI
    public void OnRetryButtonPressed()
    {
        if (retryMenuPanel != null)
        {
            retryMenuPanel.SetActive(false);
        }

        if (playerHealthReference != null)
        {
            playerHealthReference.Respawn();
        }
        else
        {
            // Fallback en caso de no tener referencia directa: buscar al personaje
            PlayerHealth player = FindAnyObjectByType<PlayerHealth>();
            if (player != null) player.Respawn();
        }
    }

    // Método asignado al botón "Salir" del Canvas UI
    public void OnQuitButtonPressed()
    {
        // Si hay menú principal se carga por escena, o cerramos la aplicación
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Carga la escena principal en el índice 0 de Build Settings
    }
}