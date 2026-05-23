using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadingManager
{
    public static string targetSceneName;

    public static void LoadScene(string sceneName)
    {
        targetSceneName = sceneName;

        SceneManager.LoadScene("Loading");
    }
}