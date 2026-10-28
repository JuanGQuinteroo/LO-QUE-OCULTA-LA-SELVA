using UnityEngine;

public class InstantDeathZone : MonoBehaviour
{
    [Header("UI Feedback")]
    [Tooltip("Ilustración en Pixel Art del mito atacando que se enviará a la pantalla de muerte")]
    [SerializeField] private Sprite mythIllustration;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ExecuteDeath(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ExecuteDeath(collision.gameObject);
    }

    private void ExecuteDeath(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Die(mythIllustration);
            }
        }
    }
}