using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToHomeButton : MonoBehaviour
{
    [Header("Home Scene")]

    [SerializeField]
    private string homeSceneName = "MainMenu";

    public void BackToHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}