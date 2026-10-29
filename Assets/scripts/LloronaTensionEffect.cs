using UnityEngine;
using UnityEngine.Rendering;

public class LloronaTensionEffect : MonoBehaviour
{
    [Header("Referencias UI / Efectos")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LloronaAI llorona;

    [Header("Rangos de Tensión")]
    [Tooltip("Distancia a la que empieza a ponerse en blanco y negro")]
    [SerializeField] private float maxDistance = 10f;
    [Tooltip("Distancia donde el efecto es 100% intenso")]
    [SerializeField] private float minDistance = 3f;

    private void Awake()
    {
        if (llorona == null) llorona = GetComponent<LloronaAI>();
    }

    private void Update()
    {
        if (postProcessVolume == null || playerTransform == null) return;

        // Inactiva (antes del diálogo) o detenida: sin efecto
        if (llorona != null && !llorona.IsActive)
        {
            postProcessVolume.weight = 0f;
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        float tensionFactor = Mathf.InverseLerp(maxDistance, minDistance, distance);
        postProcessVolume.weight = Mathf.Clamp01(tensionFactor);
    }
}
