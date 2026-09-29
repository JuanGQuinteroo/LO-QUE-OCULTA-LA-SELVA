using UnityEngine;

public class CentralCamera : MonoBehaviour
{
    public GameObject Miguel;

    [Tooltip("Qué tan suave sigue la cámara a Miguel. Más alto = más lento/suave, más bajo = más pegado.")]
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 velocity; // Uso interno de SmoothDamp, no lo toques manualmente

    private void LateUpdate()
    {
        // LateUpdate corre DESPUÉS de que Miguel ya se movió en este frame,
        // así la cámara nunca queda "un frame atrás" (evita temblores/jitter)
        if (Miguel == null) return;

        Vector3 desiredPosition = new Vector3(Miguel.transform.position.x, Miguel.transform.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}