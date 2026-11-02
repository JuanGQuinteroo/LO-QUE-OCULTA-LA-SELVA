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

    private bool currentlyChasing = false;
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
        // Buscamos a TODAS las Lloronas que existan en la escena
        LloronaAI[] allLloronas = FindObjectsByType<LloronaAI>(FindObjectsSortMode.None);

        bool isAnyChasing = false;

        // Si CUALQUIERA de ellas te está persiguiendo, activamos la alerta
        foreach (var llorona in allLloronas)
        {
            if (llorona != null && llorona.IsChasing)
            {
                isAnyChasing = true;
                break;
            }
        }

        // Detectar cambio de estado (de calma a persecución o viceversa)
        if (isAnyChasing != currentlyChasing)
        {
            currentlyChasing = isAnyChasing;

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
            Debug.Log("<color=red><b>[MusicManager]</b> ¡La Llorona te persigue! Entra música de persecución.</color>");

            // Asegurar que la de persecución esté sonando
            if (chaseSource != null)
            {
                chaseSource.loop = true;
                if (!chaseSource.isPlaying) chaseSource.Play();
            }
        }
        else
        {
            Debug.Log("<color=green><b>[MusicManager]</b> Escape exitoso. Volviendo a música ambiental.</color>");

            // Reanudar la ambiental si estaba en pausa
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

        // Asegurar volúmenes finales
        if (ambientSource != null)
        {
            ambientSource.volume = targetAmbientVol;
            if (chasing) ambientSource.Pause(); // Se pausa para continuar donde quedó
        }

        if (chaseSource != null)
        {
            chaseSource.volume = targetChaseVol;
            if (!chasing) chaseSource.Stop(); // Se apaga completamente al escapar
        }
    }
}