using UnityEngine;

public class BackToHomeButton : MonoBehaviour
{
    [Header("Home Scene")]

    [SerializeField]
    private string homeSceneName = "MainMenu";

    public void BackToHome()
    {
        LoadingManager.LoadScene(homeSceneName);
    }
}