using UnityEngine;

public class PlayButton : MonoBehaviour
{
    [Header("Game Scene")]

    [SerializeField]
    private string gameplaySceneName = "Gameplay";

    public void PlayGame()
    {
        LoadingManager.LoadScene(gameplaySceneName);
    }
}