using UnityEngine;
using UnityEngine.Rendering;

public class LloronaTensionEffect : MonoBehaviour
{
    [Header("Referencias UI / Efectos")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Transform playerTransform;

    [Header("Rangos de Tensión")]
    [Tooltip("Distancia a la que empieza a ponerse en blanco y negro")]
    [SerializeField] private float maxDistance = 10f;
    [Tooltip("Distancia donde el efecto es 100% intenso")]
    [SerializeField] private float minDistance = 3f;

    private void Update()
    {
        if (postProcessVolume == null || playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Calcula un valor entre 0 y 1 dependiendo de qué tan cerca está Miguel
        float tensionFactor = Mathf.InverseLerp(maxDistance, minDistance, distance);

        // Aplica la intensidad al Post-Processing (0 = Normal, 1 = Blanco y Negro + Blur)
        postProcessVolume.weight = Mathf.Clamp01(tensionFactor);
    }
}