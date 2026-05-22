using UnityEngine;
using UnityEngine.UI;

public class ReplayButtonHandler : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnReplayClick);
        }
        else
        {
            Debug.LogError("ReplayButtonHandler: No Button component found!");
        }
    }
    
    void OnReplayClick()
    {
        Debug.Log("=== REPLAY BUTTON PRESSED ===");
        
        // Tìm PopupManager
        PopupManager popupManager = FindObjectOfType<PopupManager>();
        if (popupManager != null)
        {
            // Gọi method replay
            popupManager.OnReplayButtonClick();
        }
        else
        {
            Debug.LogError("Cannot find PopupManager!");
            
            // Fallback: tự xử lý
            GameObject pausePopup = GameObject.Find("PausePopup");
            if (pausePopup != null)
                pausePopup.SetActive(false);
            
            LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
            if (levelLoader != null)
                levelLoader.ReloadCurrentLevel();
            
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ReplayGame();
        }
    }
}