using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource chaseSource;

    [Header("Configuración de Transición")]
    [Tooltip("Tiempo en segundos que tarda la transición de volumen")]
    [SerializeField] private float fadeDuration = 0.8f;

    [Header("Persistencia de Peligro")]
    [Tooltip("Distancia a la que debe alejarse La Llorona para que la música vuelva a la calma aunque estés escondido")]
    [SerializeField] private float safeDistance = 8f;

    private bool currentlyChasing = false;
    private bool inAlertMode = false; // Mantiene la música sonando en el escondite
    private Coroutine transitionCoroutine;

    private float maxAmbientVolume = 0.5f;
    private float maxChaseVolume = 1f;

    void Awake()
    {
        // 1. Si no se arrastraron a mano, se detectan automáticamente
        AudioSource[] sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            if (ambientSource == null) ambientSource = sources[0];
            if (chaseSource == null) chaseSource = sources[1];
        }

        // 2. Configurar la música ambiental
        if (ambientSource != null)
        {
            maxAmbientVolume = ambientSource.volume > 0 ? ambientSource.volume : 0.5f;
            ambientSource.loop = true;
            if (!ambientSource.isPlaying) ambientSource.Play();
        }

        // 3. Configurar la música de persecución
        if (chaseSource != null)
        {
            maxChaseVolume = chaseSource.volume > 0 ? chaseSource.volume : 1f;
            chaseSource.volume = 0f; // Silenciada al inicio
            chaseSource.loop = true; // Bucle obligatorio
        }
    }

    void Update()
    {
        LloronaAI[] allLloronas = FindObjectsByType<LloronaAI>(FindObjectsSortMode.None);
        Transform player = FindFirstObjectByType<miguel_moviments>()?.transform;

        bool isAnyChasing = false;
        bool isAnyClose = false;

        // Comprobamos el estado de las Lloronas vivas
        foreach (var llorona in allLloronas)
        {
            if (llorona != null && llorona.IsActive)
            {
                if (llorona.IsChasing)
                {
                    isAnyChasing = true;
                }

                if (player != null)
                {
                    float dist = Vector2.Distance(player.position, llorona.transform.position);
                    if (dist <= safeDistance)
                    {
                        isAnyClose = true;
                    }
                }
            }
        }

        // 1. Si te detecta y empieza a perseguir, entramos en MODO ALERTA
        if (isAnyChasing)
        {
            inAlertMode = true;
        }

        // 2. Solo salimos del MODO ALERTA si:
        // Ya no te persigue Y TAMBIÉN ya se alejó a más de la distancia segura (o murió)
        if (inAlertMode && !isAnyChasing && !isAnyClose)
        {
            inAlertMode = false;
        }

        // 3. La música responderá a si estamos en Alerta
        bool shouldPlayChase = inAlertMode;

        if (shouldPlayChase != currentlyChasing)
        {
            currentlyChasing = shouldPlayChase;

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(CrossfadeRoutine(currentlyChasing));
        }
    }

    private IEnumerator CrossfadeRoutine(bool chasing)
    {
        float elapsed = 0f;

        float startAmbientVol = ambientSource != null ? ambientSource.volume : 0f;
        float targetAmbientVol = chasing ? 0f : maxAmbientVolume;

        float startChaseVol = chaseSource != null ? chaseSource.volume : 0f;
        float targetChaseVol = chasing ? maxChaseVolume : 0f;

        if (chasing)
        {
            Debug.Log("<color=red><b>[MusicManager]</b> ¡Alerta activada! Entra música de tensión.</color>");

            if (chaseSource != null)
            {
                chaseSource.loop = true;
                if (!chaseSource.isPlaying) chaseSource.Play();
            }
        }
        else
        {
            Debug.Log("<color=green><b>[MusicManager]</b> Peligro superado. Volviendo a música ambiental.</color>");

            if (ambientSource != null && !ambientSource.isPlaying)
            {
                ambientSource.UnPause();
            }
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            if (ambientSource != null)
                ambientSource.volume = Mathf.Lerp(startAmbientVol, targetAmbientVol, t);

            if (chaseSource != null)
                chaseSource.volume = Mathf.Lerp(startChaseVol, targetChaseVol, t);

            yield return null;
        }

        if (ambientSource != null)
        {
            ambientSource.volume = targetAmbientVol;
            if (chasing) ambientSource.Pause();
        }

        if (chaseSource != null)
        {
            chaseSource.volume = targetChaseVol;
            if (!chasing) chaseSource.Stop();
        }
    }
}