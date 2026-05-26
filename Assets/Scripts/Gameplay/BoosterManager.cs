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
        disable2.gameObject.SetActive(currentLevel < 3);
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

    private bool IsSlotBlockedByFoodLock(TrayFoodSlot slot)
    {
        if (slot == null) return false;

        Transform trayTransform = slot.transform.parent;
        if (trayTransform == null) return false;

        FoodLockController foodLock = trayTransform.GetComponentInChildren<FoodLockController>();
        if (foodLock != null && foodLock.IsLocked())
        {
            return true;
        }

        return false;
    }

    // ==================== CLASS ĐỂ LƯU THÔNG TIN FOOD SLOT ====================
    private class FoodItemInfo
    {
        public TrayFoodSlot slot;      // Cho visible items
        public SpriteRenderer spriteRenderer; // Cho hidden items (trong PlateFood)
        public Transform parent;        // Để biết vị trí
        public bool isHidden;           // true nếu là hidden, false nếu là visible
        public TrayManager tray;        // Tray cha (để check tray state sau khi shuffle)
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

        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager != null && !scoreManager.IsGameActive())
        {
            Debug.Log("Game chưa bắt đầu hoặc đã kết thúc!");
            return;
        }

        TrayFoodSlot[] allSlots = FindObjectsOfType<TrayFoodSlot>();

        Dictionary<string, List<TrayFoodSlot>> foodGroups = new Dictionary<string, List<TrayFoodSlot>>();

        foreach (TrayFoodSlot slot in allSlots)
        {
            if (slot == null) continue;
            if (slot.IsEmpty()) continue;

            if (IsSlotBlockedByFoodLock(slot))
            {
                Debug.Log($"Bỏ qua slot tại tray {slot.GetTray()?.name} vì đang bị FoodLock khóa");
                continue;
            }

            Sprite sprite = slot.GetSprite();
            if (sprite == null) continue;

            string foodKey = sprite.name;

            if (!foodGroups.ContainsKey(foodKey))
                foodGroups[foodKey] = new List<TrayFoodSlot>();

            foodGroups[foodKey].Add(slot);
        }

        List<string> validFoodTypes = new List<string>();
        foreach (var kvp in foodGroups)
        {
            if (kvp.Value.Count >= 3)
                validFoodTypes.Add(kvp.Key);
        }

        if (validFoodTypes.Count == 0)
        {
            Debug.Log("Không có loại thức ăn nào đủ 3 cái để clear!");
            return;
        }

        string selectedFoodType = validFoodTypes[Random.Range(0, validFoodTypes.Count)];
        List<TrayFoodSlot> slotsToClear = foodGroups[selectedFoodType];
        List<TrayFoodSlot> selectedSlots = slotsToClear.Take(3).ToList();

        booster1--;
        SaveBoosterData();
        RefreshUI();

        StartCoroutine(HighlightAndClearSlots(selectedSlots));

        if (scoreManager != null)
            scoreManager.AddStep();

        Debug.Log($"Used RandomTripleClear on food type: {selectedFoodType}, cleared {selectedSlots.Count} items");
    }

    private System.Collections.IEnumerator HighlightAndClearSlots(List<TrayFoodSlot> slots)
    {
        List<DragDropManager> dragDropManagers = new List<DragDropManager>();
        List<Vector3> originalScales = new List<Vector3>();

        foreach (TrayFoodSlot slot in slots)
        {
            if (slot == null) continue;

            DragDropManager dragDrop = slot.GetComponentInChildren<DragDropManager>();
            if (dragDrop != null)
            {
                dragDropManagers.Add(dragDrop);
                originalScales.Add(dragDrop.transform.localScale);
                dragDrop.transform.localScale = originalScales[originalScales.Count - 1] * 1.2f;
            }
        }

        yield return new WaitForSeconds(1.5f);

        foreach (TrayFoodSlot slot in slots)
        {
            if (slot == null) continue;

            DragDropManager dragDrop = slot.GetComponentInChildren<DragDropManager>();
            if (dragDrop != null)
            {
                dragDrop.ResetToOriginalPosition();
            }

            slot.SetSprite(null);

            TrayManager tray = slot.GetTray();
            if (tray != null)
                tray.CheckTrayState();
        }

        for (int i = 0; i < dragDropManagers.Count; i++)
        {
            if (dragDropManagers[i] != null)
            {
                dragDropManagers[i].transform.localScale = originalScales[i];
            }
        }
    }

    // ==================== BOOSTER 2: Shuffle (cả visible và hidden) ====================
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

        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager != null && !scoreManager.IsGameActive())
        {
            Debug.Log("Game chưa bắt đầu hoặc đã kết thúc!");
            return;
        }

        bool shuffleSuccess = PerformFullShuffle();

        if (!shuffleSuccess)
        {
            Debug.Log("Không thể shuffle: không có đủ food items để hoán đổi!");
            return;
        }

        booster2--;
        SaveBoosterData();
        RefreshUI();

        Debug.Log("Used Shuffle booster! (đã shuffle cả visible và hidden)");
    }

    private bool PerformFullShuffle()
    {
        List<FoodItemInfo> allFoodItems = new List<FoodItemInfo>();
        List<Sprite> allSprites = new List<Sprite>();

        // 1. Thu thập tất cả VISIBLE items (TrayFoodSlot)
        TrayFoodSlot[] allSlots = FindObjectsOfType<TrayFoodSlot>();

        foreach (TrayFoodSlot slot in allSlots)
        {
            if (slot == null) continue;
            if (slot.IsEmpty()) continue;

            // Skip slots blocked by FoodLock
            if (IsSlotBlockedByFoodLock(slot))
            {
                Debug.Log($"Bỏ qua visible slot tại tray {slot.GetTray()?.name} vì đang bị FoodLock khóa");
                continue;
            }

            Sprite sprite = slot.GetSprite();
            if (sprite == null) continue;

            FoodItemInfo info = new FoodItemInfo();
            info.slot = slot;
            info.spriteRenderer = null;
            info.parent = slot.transform;
            info.isHidden = false;
            info.tray = slot.GetTray();

            allFoodItems.Add(info);
            allSprites.Add(sprite);
        }

        // 2. Thu thập tất cả HIDDEN items (trong PlateContainer -> PlateFood)
        // Tìm tất cả các PlateFood trong scene
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (t.name == "PlateFood" && t.childCount > 0)
            {
                // PlateFood có 1 child là SpriteRenderer (hoặc Image)
                SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    // Kiểm tra xem có bị FoodLock ảnh hưởng không?
                    // Hidden items nằm trong PlateContainer, cần check Tray cha
                    Transform current = t.parent;
                    Transform trayTransform = null;
                    while (current != null)
                    {
                        if (current.name.Contains("Tray") && current.parent != null && current.parent.name.Contains("TrayRow"))
                        {
                            trayTransform = current;
                            break;
                        }
                        current = current.parent;
                    }

                    // Check FoodLock trên tray này
                    bool isBlocked = false;
                    if (trayTransform != null)
                    {
                        FoodLockController foodLock = trayTransform.GetComponentInChildren<FoodLockController>();
                        if (foodLock != null && foodLock.IsLocked())
                        {
                            isBlocked = true;
                            Debug.Log($"Bỏ qua hidden item tại {trayTransform.name} vì đang bị FoodLock khóa");
                        }
                    }

                    if (!isBlocked)
                    {
                        FoodItemInfo info = new FoodItemInfo();
                        info.slot = null;
                        info.spriteRenderer = sr;
                        info.parent = t;
                        info.isHidden = true;
                        info.tray = trayTransform?.GetComponent<TrayManager>();

                        allFoodItems.Add(info);
                        allSprites.Add(sr.sprite);
                    }
                }
            }
        }

        // Need at least 2 items to shuffle
        if (allFoodItems.Count < 2)
        {
            Debug.Log($"Không đủ items để shuffle: chỉ có {allFoodItems.Count} items");
            return false;
        }

        // Store DragDropManager references for visible items (for animation)
        List<DragDropManager> dragDropManagers = new List<DragDropManager>();
        List<Vector3> originalScales = new List<Vector3>();

        // Shuffle the sprites list (Fisher-Yates)
        for (int i = 0; i < allSprites.Count; i++)
        {
            int randomIndex = Random.Range(i, allSprites.Count);
            Sprite temp = allSprites[i];
            allSprites[i] = allSprites[randomIndex];
            allSprites[randomIndex] = temp;
        }

        // Apply shuffled sprites back to items
        for (int i = 0; i < allFoodItems.Count; i++)
        {
            FoodItemInfo info = allFoodItems[i];
            Sprite newSprite = allSprites[i];

            if (info.isHidden)
            {
                // Hidden item: update SpriteRenderer
                if (info.spriteRenderer != null)
                {
                    info.spriteRenderer.sprite = newSprite;
                    // Nếu sprite null thì disable, enable nếu có sprite
                    info.spriteRenderer.enabled = (newSprite != null);
                }
            }
            else
            {
                // Visible item: update TrayFoodSlot
                if (info.slot != null)
                {
                    // Get DragDropManager for animation
                    DragDropManager dragDrop = info.slot.GetComponentInChildren<DragDropManager>();
                    if (dragDrop != null)
                    {
                        dragDropManagers.Add(dragDrop);
                        originalScales.Add(dragDrop.transform.localScale);
                        dragDrop.transform.localScale = originalScales[originalScales.Count - 1] * 1.2f;
                    }

                    info.slot.SetSprite(newSprite);
                }
            }
        }

        // Check tray states after shuffle (có thể có tray hoàn thành sau khi shuffle)
        HashSet<TrayManager> traysToCheck = new HashSet<TrayManager>();
        foreach (FoodItemInfo info in allFoodItems)
        {
            if (info.tray != null)
                traysToCheck.Add(info.tray);
        }

        foreach (TrayManager tray in traysToCheck)
        {
            if (tray != null)
                tray.CheckTrayState();
        }

        // Start coroutine to restore scales after animation
        StartCoroutine(RestoreDragDropScales(dragDropManagers, originalScales));

        Debug.Log($"Shuffle completed: shuffled {allFoodItems.Count} items (visible + hidden)");
        return true;
    }

    private System.Collections.IEnumerator RestoreDragDropScales(List<DragDropManager> dragDropManagers, List<Vector3> originalScales)
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < dragDropManagers.Count; i++)
        {
            if (dragDropManagers[i] != null)
            {
                dragDropManagers[i].transform.localScale = originalScales[i];
            }
        }
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