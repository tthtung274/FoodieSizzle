using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuPlayButton : MonoBehaviour
{
    public Button playButton;
    public TMP_Text buttonText;
    public string levelPrefix = "Cấp độ ";
    public string defaultText = "Play";

    void Start()
    {
        if (playButton == null)
            playButton = GetComponent<Button>();

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        UpdateButtonText();
    }

    void UpdateButtonText()
    {
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        if (buttonText != null)
        {
            if (currentLevel >= 1)
            {
                buttonText.text = levelPrefix + currentLevel;
                Debug.Log("Nút hiển thị: " + buttonText.text); // Kiểm tra log
            }
            else
            {
                buttonText.text = defaultText;
            }
        }
    }

    void OnPlayButtonClicked()
    {
        int levelToPlay = PlayerPrefs.GetInt("CurrentLevel", 1);
        PlayerPrefs.SetInt("LevelToLoad", levelToPlay);
        PlayerPrefs.Save();

        Debug.Log("Đang chơi màn: " + levelToPlay);
        LoadingManager.LoadScene("Gameplay"); // Đổi tên scene của bạn
    }
}