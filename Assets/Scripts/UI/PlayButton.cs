using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    [Header("Game Scene")]

    [SerializeField]
    private string gameplaySceneName = "Gameplay";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }
}
