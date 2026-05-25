using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class BoosterManager : MonoBehaviour
{
    [Header("Level Text")]
    [SerializeField] private TMP_Text levelText;

    [Header("Disable Images")]
    [SerializeField] private Image disable1;
    [SerializeField] private Image disable2;
    [SerializeField] private Image disable3;
    [SerializeField] private Image disable4;

    [Header("Booster Count Text")]
    [SerializeField] private TMP_Text boosterText1;
    [SerializeField] private TMP_Text boosterText2;
    [SerializeField] private TMP_Text boosterText3;
    [SerializeField] private TMP_Text boosterText4;

    [Header("Number Parent")]
    [SerializeField] private GameObject number1;
    [SerializeField] private GameObject number2;
    [SerializeField] private GameObject number3;
    [SerializeField] private GameObject number4;

    [Header("Plus Images")]
    [SerializeField] private Image plusImage1;
    [SerializeField] private Image plusImage2;
    [SerializeField] private Image plusImage3;
    [SerializeField] private Image plusImage4;

    [Header("References")]
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private ScoreManager scoreManager;

    private int booster1;
    private int booster2;
    private int booster3;
    private int booster4;

    private const string Booster1Key = "Booster1";
    private const string Booster2Key = "Booster2";
    private const string Booster3Key = "Booster3";
    private const string Booster4Key = "Booster4";

    // Level booster availability
    private bool hasRandomTripleClear = false;
    private bool hasShuffle = false;
    private bool hasFoodPurge = false;
    private bool hasFlameBooster = false;

    private void Start()
    {
        LoadBoosterData();
        RefreshUI();
    }

    private void Update()
    {
        UpdateBoosterUnlockByLevel();
        UpdateBoosterAvailabilityByMap();
    }

    private void LoadBoosterData()
    {
        booster1 = PlayerPrefs.GetInt(Booster1Key, 1);
        booster2 = PlayerPrefs.GetInt(Booster2Key, 1);
        booster3 = PlayerPrefs.GetInt(Booster3Key, 1);
        booster4 = PlayerPrefs.GetInt(Booster4Key, 1);
    }

    private void SaveBoosterData()
    {
        PlayerPrefs.SetInt(Booster1Key, booster1);
        PlayerPrefs.SetInt(Booster2Key, booster2);
        PlayerPrefs.SetInt(Booster3Key, booster3);
        PlayerPrefs.SetInt(Booster4Key, booster4);
        PlayerPrefs.Save();
    }

    private void UpdateBoosterUnlockByLevel()
    {
        int currentLevel = GetLevelFromText();

        disable1.gameObject.SetActive(currentLevel < 2);
        disable2.gameObject.SetActive(currentLevel < 6);
        disable3.gameObject.SetActive(currentLevel < 8);
        disable4.gameObject.SetActive(currentLevel < 20);
    }

    private void UpdateBoosterAvailabilityByMap()
    {
        if (levelLoader == null)
        {
            levelLoader = FindObjectOfType<LevelLoader>();
            if (levelLoader == null) return;
        }

        LevelData currentLevelData = levelLoader.GetCurrentLevelData();

        if (currentLevelData == null || currentLevelData.booster == null)
        {
            hasRandomTripleClear = false;
            hasShuffle = false;
            hasFoodPurge = false;
            hasFlameBooster = false;
            return;
        }

        hasRandomTripleClear = currentLevelData.booster.Any(b => b.type == "RandomTripleClear");
        hasShuffle = currentLevelData.booster.Any(b => b.type == "Shuffle");
        hasFoodPurge = currentLevelData.booster.Any(b => b.type == "FoodPurge");
        hasFlameBooster = currentLevelData.booster.Any(b => b.type == "FlameBooster");

        // Update disable images based on map availability
        if (disable1 != null) disable1.gameObject.SetActive(!hasRandomTripleClear);
        if (disable2 != null) disable2.gameObject.SetActive(!hasShuffle);
        if (disable3 != null) disable3.gameObject.SetActive(!hasFoodPurge);
        if (disable4 != null) disable4.gameObject.SetActive(!hasFlameBooster);
    }

    private void RefreshUI()
    {
        UpdateBoosterText();
        UpdateBoosterButtons();
    }

    private void UpdateBoosterText()
    {
        if (boosterText1 != null) boosterText1.text = booster1.ToString();
        if (boosterText2 != null) boosterText2.text = booster2.ToString();
        if (boosterText3 != null) boosterText3.text = booster3.ToString();
        if (boosterText4 != null) boosterText4.text = booster4.ToString();
    }

    private void UpdateBoosterButtons()
    {
        SetBoosterUI(booster1, number1, plusImage1);
        SetBoosterUI(booster2, number2, plusImage2);
        SetBoosterUI(booster3, number3, plusImage3);
        SetBoosterUI(booster4, number4, plusImage4);
    }

    private void SetBoosterUI(int boosterCount, GameObject numberObject, Image plusImage)
    {
        bool hasBooster = boosterCount > 0;

        if (numberObject != null)
            numberObject.SetActive(hasBooster);

        if (plusImage != null)
            plusImage.gameObject.SetActive(!hasBooster);
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

    // Check if a slot is blocked by FoodLock (and FoodLock is still active)
    private bool IsSlotBlockedByFoodLock(TrayFoodSlot slot)
    {
        if (slot == null) return false;

        // Get the parent tray
        Transform trayTransform = slot.transform.parent;
        if (trayTransform == null) return false;

        // Check if there's an active FoodLock in this tray
        FoodLockController foodLock = trayTransform.GetComponentInChildren<FoodLockController>();
        if (foodLock != null && foodLock.IsLocked())
        {
            // FoodLock is still active, slot is blocked
            return true;
        }

        return false;
    }

    // ==================== BOOSTER 1: RandomTripleClear ====================
    public void OnBooster1Click()
    {
        if (!hasRandomTripleClear)
        {
            Debug.Log("RandomTripleClear không có trong map này!");
            return;
        }

        if (booster1 <= 0)
        {
            Debug.Log("Không đủ booster 1!");
            return;
        }

        // Check if game is active
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager != null && !scoreManager.IsGameActive())
        {
            Debug.Log("Game chưa bắt đầu hoặc đã kết thúc!");
            return;
        }

        // Find all TrayFoodSlot in scene
        TrayFoodSlot[] allSlots = FindObjectsOfType<TrayFoodSlot>();

        // Group by food type, EXCLUDING slots that are blocked by FoodLock
        Dictionary<string, List<TrayFoodSlot>> foodGroups = new Dictionary<string, List<TrayFoodSlot>>();

        foreach (TrayFoodSlot slot in allSlots)
        {
            if (slot == null) continue;
            if (slot.IsEmpty()) continue;

            // IMPORTANT: Skip slots that are blocked by FoodLock
            if (IsSlotBlockedByFoodLock(slot))
            {
                Debug.Log($"Bỏ qua slot tại tray {slot.GetTray()?.name} vì đang bị FoodLock khóa");
                continue;
            }

            Sprite sprite = slot.GetSprite();
            if (sprite == null) continue;

            // Use sprite name as key
            string foodKey = sprite.name;

            if (!foodGroups.ContainsKey(foodKey))
                foodGroups[foodKey] = new List<TrayFoodSlot>();

            foodGroups[foodKey].Add(slot);
        }

        // Only find food types that have AT LEAST 3 slots
        List<string> validFoodTypes = new List<string>();
        foreach (var kvp in foodGroups)
        {
            if (kvp.Value.Count >= 3)
                validFoodTypes.Add(kvp.Key);
        }

        if (validFoodTypes.Count == 0)
        {
            Debug.Log("Không có loại thức ăn nào đủ 3 cái để clear! (Không tính các ô đang bị FoodLock khóa)");
            return;  // Không trừ booster, không làm gì cả
        }

        // Random pick one food type
        string selectedFoodType = validFoodTypes[Random.Range(0, validFoodTypes.Count)];
        List<TrayFoodSlot> slotsToClear = foodGroups[selectedFoodType];

        // Take exactly 3 slots (first 3)
        List<TrayFoodSlot> selectedSlots = slotsToClear.Take(3).ToList();

        // Use booster (decrease count) - ONLY HERE AFTER VALIDATION
        booster1--;
        SaveBoosterData();
        RefreshUI();

        // Show effect and clear
        StartCoroutine(HighlightAndClearSlots(selectedSlots));

        // Add step
        if (scoreManager != null)
            scoreManager.AddStep();

        Debug.Log($"Used RandomTripleClear on food type: {selectedFoodType}, cleared {selectedSlots.Count} items");
    }

    private System.Collections.IEnumerator HighlightAndClearSlots(List<TrayFoodSlot> slots)
    {
        // Store original scale and get DragDropManager components
        List<DragDropManager> dragDropManagers = new List<DragDropManager>();
        List<Vector3> originalScales = new List<Vector3>();

        foreach (TrayFoodSlot slot in slots)
        {
            if (slot == null) continue;

            // Get DragDropManager from the food object (child of slot)
            DragDropManager dragDrop = slot.GetComponentInChildren<DragDropManager>();
            if (dragDrop != null)
            {
                dragDropManagers.Add(dragDrop);
                originalScales.Add(dragDrop.transform.localScale);

                // Scale up to 1.5x
                dragDrop.transform.localScale = originalScales[originalScales.Count - 1] * 1.2f;
            }
        }

        // Wait for 1.5 seconds (active state)
        yield return new WaitForSeconds(1.5f);

        // Clear the slots
        foreach (TrayFoodSlot slot in slots)
        {
            if (slot == null) continue;

            // Get the DragDropManager on the food object and reset it
            DragDropManager dragDrop = slot.GetComponentInChildren<DragDropManager>();
            if (dragDrop != null)
            {
                dragDrop.ResetToOriginalPosition();
            }

            // Clear the slot
            slot.SetSprite(null);

            // Check trays after clearing
            TrayManager tray = slot.GetTray();
            if (tray != null)
                tray.CheckTrayState();
        }

        // Restore scales
        for (int i = 0; i < dragDropManagers.Count; i++)
        {
            if (dragDropManagers[i] != null)
            {
                dragDropManagers[i].transform.localScale = originalScales[i];
            }
        }
    }

    // ==================== BOOSTER 2: Shuffle (TODO) ====================
    public void OnBooster2Click()
    {
        if (!hasShuffle)
        {
            Debug.Log("Shuffle không có trong map này!");
            return;
        }

        if (booster2 <= 0)
        {
            Debug.Log("Không đủ booster 2!");
            return;
        }

        // TODO: Implement Shuffle logic later
        Debug.Log("Shuffle chưa được implement!");
    }

    // ==================== BOOSTER 3: FoodPurge (TODO) ====================
    public void OnBooster3Click()
    {
        if (!hasFoodPurge)
        {
            Debug.Log("FoodPurge không có trong map này!");
            return;
        }

        if (booster3 <= 0)
        {
            Debug.Log("Không đủ booster 3!");
            return;
        }

        // TODO: Implement FoodPurge logic later
        Debug.Log("FoodPurge chưa được implement!");
    }

    // ==================== BOOSTER 4: FlameBooster (TODO) ====================
    public void OnBooster4Click()
    {
        if (!hasFlameBooster)
        {
            Debug.Log("FlameBooster không có trong map này!");
            return;
        }

        if (booster4 <= 0)
        {
            Debug.Log("Không đủ booster 4!");
            return;
        }

        // TODO: Implement FlameBooster logic later
        Debug.Log("FlameBooster chưa được implement!");
    }

    // ==================== Add Booster Methods ====================
    public void AddBooster1(int amount)
    {
        booster1 += amount;
        SaveBoosterData();
        RefreshUI();
    }

    public void AddBooster2(int amount)
    {
        booster2 += amount;
        SaveBoosterData();
        RefreshUI();
    }

    public void AddBooster3(int amount)
    {
        booster3 += amount;
        SaveBoosterData();
        RefreshUI();
    }

    public void AddBooster4(int amount)
    {
        booster4 += amount;
        SaveBoosterData();
        RefreshUI();
    }

    // ==================== DEBUG METHODS ====================
    [ContextMenu("Reset All Boosters to 10")]
    public void ResetAllBoosters()
    {
        booster1 = 10;
        booster2 = 10;
        booster3 = 10;
        booster4 = 10;
        SaveBoosterData();
        RefreshUI();
        Debug.Log("All boosters reset to 10!");
    }
}