using UnityEngine;
using UnityEngine.UI;

// Ponlo en la escena GameOver y arrastra el Image donde se mostrará la ilustración
public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private Image illustrationImage;

    private void Start()
    {
        if (illustrationImage != null && GameOverData.Illustration != null)
        {
            illustrationImage.sprite = GameOverData.Illustration;
        }
    }
}
