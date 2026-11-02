using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("Componentes UI")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Image mythImageDisplay;

    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealthReference;

    private void OnEnable()
    {
        // Nos suscribimos al evento estático cuando este objeto se activa
        PlayerHealth.OnPlayerDied += ShowDeathScreen;
    }

    private void OnDisable()
    {
        // Cancelamos la suscripción para evitar fugas de memoria (Memory Leaks)
        PlayerHealth.OnPlayerDied -= ShowDeathScreen;
    }

    private void Start()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
    }

    private void ShowDeathScreen(Sprite mythIllustration)
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        if (mythImageDisplay != null)
        {
            if (mythIllustration != null)
            {
                mythImageDisplay.sprite = mythIllustration;
                mythImageDisplay.enabled = true;
            }
            else
            {
                mythImageDisplay.enabled = false;
            }
        }
    }

    // Método asignado al botón "Reintentar" del Canvas UI
    public void OnRetryButtonPressed()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
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