// BoosterManager.cs

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterManager : MonoBehaviour
{
    [Header("Level Text")]
    [SerializeField] private TMP_Text levelText;

    [Header("Disable Images")]
    [SerializeField] private Image disable1;
    [SerializeField] private Image disable2;
    [SerializeField] private Image disable3;
    [SerializeField] private Image disable4;

    private void Update()
    {
        UpdateBoosterUI();
    }

    private void UpdateBoosterUI()
    {
        int currentLevel = GetLevelFromText();

        disable1.gameObject.SetActive(currentLevel < 2);
        disable2.gameObject.SetActive(currentLevel < 6);
        disable3.gameObject.SetActive(currentLevel < 8);
        disable4.gameObject.SetActive(currentLevel < 20);
    }

    private int GetLevelFromText()
    {
        if (levelText == null || string.IsNullOrEmpty(levelText.text))
            return 1;

        string levelString = levelText.text.Replace("Lv.", "").Trim();

        if (int.TryParse(levelString, out int level))
            return level;

        return 1;
    }
}