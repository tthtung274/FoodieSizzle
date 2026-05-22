using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [Header("Popup")]
    public GameObject popupSetting;
    public GameObject popupProfile;
    public GameObject popupPause;

    [Header("References")]
    public LevelLoader levelLoader;

    private void Start()
    {
        if (levelLoader == null)
        {
            levelLoader = FindObjectOfType<LevelLoader>();
        }
    }

    public void OpenPopupSetting()
    {
        if (popupSetting != null)
        {
            popupSetting.SetActive(true);
        }
    }

    public void ClosePopupSetting()
    {
        if (popupSetting != null)
        {
            popupSetting.SetActive(false);
        }
    }

    public void OpenPopupProfile()
    {
        if (popupProfile != null)
        {
            popupProfile.SetActive(true);
        }
    }

    public void ClosePopupProfile()
    {
        if (popupProfile != null)
        {
            popupProfile.SetActive(false);
        }
    }

    public void OpenPopupPause()
    {
        if (popupPause != null)
        {
            popupPause.SetActive(true);
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.PauseGame();
            }
        }
    }

    public void ClosePopupPause()
    {
        if (popupPause != null)
        {
            popupPause.SetActive(false);
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResumeGame();
            }
        }
    }

    // Method này để gán cho Button Replay
    public void OnReplayButtonClick()
    {
        Debug.Log("=== REPLAY BUTTON CLICKED ===");

        // Đóng popup pause
        if (popupPause != null)
        {
            popupPause.SetActive(false);
            Debug.Log("Closed pause popup");
        }

        // Reset ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ReplayGame();
            Debug.Log("ScoreManager reset");
        }

        // Reload level
        if (levelLoader != null)
        {
            levelLoader.ReloadCurrentLevel();
            Debug.Log("Level reloaded");
        }
        else
        {
            levelLoader = FindObjectOfType<LevelLoader>();
            if (levelLoader != null)
            {
                levelLoader.ReloadCurrentLevel();
                Debug.Log("Level reloaded (found automatically)");
            }
            else
            {
                Debug.LogError("Cannot find LevelLoader!");
            }
        }

        Debug.Log("=== REPLAY COMPLETE ===");
    }
}