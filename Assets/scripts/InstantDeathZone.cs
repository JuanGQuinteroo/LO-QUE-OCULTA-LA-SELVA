using UnityEngine;
using UnityEngine.SceneManagement;

public class InstantDeathZone : MonoBehaviour
{
    [Header("UI Feedback")]
    [Tooltip("Ilustración en Pixel Art del mito atacando que se enviará a la pantalla de muerte")]
    [SerializeField] private Sprite mythIllustration;

    [Header("Game Over")]
    [Tooltip("Nombre exacto de la escena de Game Over (debe estar en Build Settings)")]
    [SerializeField] private string gameOverSceneName = "GameOver";

    [Header("Referencias")]
    [SerializeField] private LloronaAI llorona;

    private bool hasKilled;

    private void Awake()
    {
        if (llorona == null) llorona = GetComponent<LloronaAI>();
        if (llorona == null) llorona = GetComponentInParent<LloronaAI>();
    }

    private void OnTriggerEnter2D(Collider2D collision) { TryKill(collision.gameObject); }
    private void OnTriggerStay2D(Collider2D collision) { TryKill(collision.gameObject); }
    private void OnCollisionEnter2D(Collision2D collision) { TryKill(collision.gameObject); }
    private void OnCollisionStay2D(Collision2D collision) { TryKill(collision.gameObject); }

    private void TryKill(GameObject obj)
   {
    if (hasKilled) return;

    Debug.Log($"[DeathZone] Contacto con: {obj.name} | Tag: {obj.tag} | Llorona activa: {(llorona != null ? llorona.IsActive.ToString() : "SIN REFERENCIA")}");

    if (llorona != null && !llorona.IsActive) return;

    if (!obj.CompareTag("Player"))
    {
        Debug.Log("[DeathZone] El objeto no tiene el tag Player");
        return;
    }

    miguel_moviments miguel = obj.GetComponent<miguel_moviments>();
    if (miguel != null && miguel.IsHiding)
    {
        Debug.Log("[DeathZone] Miguel está escondido");
        return;
    }

    hasKilled = true;
    Debug.Log("[DeathZone] Cargando escena: " + gameOverSceneName);
    GameOverData.Illustration = mythIllustration;
    SceneManager.LoadScene(gameOverSceneName);
  }
}
