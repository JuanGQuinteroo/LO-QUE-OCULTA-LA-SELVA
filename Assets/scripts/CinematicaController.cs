using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CinematicaController : MonoBehaviour
{
    [SerializeField] private string escenaSiguiente = "Nivel_LaLlorona";
    private VideoPlayer videoPlayer;
    private bool cargando = false;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += AlTerminarVideo;
    }

    void Update()
    {
        // Saltar la cinemática con la barra espaciadora
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CargarSiguienteEscena();
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        CargarSiguienteEscena();
    }

    void CargarSiguienteEscena()
    {
        if (cargando) return;
        cargando = true;

        videoPlayer.loopPointReached -= AlTerminarVideo;
        SceneManager.LoadScene(escenaSiguiente);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= AlTerminarVideo;
    }
}