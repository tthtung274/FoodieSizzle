using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelLoader : MonoBehaviour
{
    [Header("Level")]
    public int currentLevel = 1;

    [Header("Tray Rows")]
    public List<Transform> trayRows;

    [Header("UI")]
    public TMP_Text levelText;
    public TMP_Text timeText;
    public TMP_Text stepText;

    [Header("Food Sprites (ID 1..30)")]
    public Sprite[] foodSprites;

    [Header("Plate Container Template")]
    public GameObject plateContainerTemplate;

    [Header("Score Manager")]
    public ScoreManager scoreManager;

    [Header("FoodLock")]
    public GameObject foodLockPrefab;

    [Header("HardLock")] // THÊM MỚI
    public GameObject hardLockPrefab;

    private Dictionary<int, Sprite> foodMap;
    private LevelData currentLevelData;
    private bool isReloading = false;
    private GameObject templateInstance;
    private bool hasAutoReloaded = false;

    // Track spawned FoodLock instances: key = trayId (1-based layout position), value = GameObject
    private Dictionary<int, GameObject> activeFoodLocks = new Dictionary<int, GameObject>();

    // Track spawned HardLock instances // THÊM MỚI
    private Dictionary<int, GameObject> activeHardLocks = new Dictionary<int, GameObject>();

    void Awake()
    {
        if (plateContainerTemplate == null)
        {
            Debug.LogError("Chưa kéo PlateContainer vào! Hãy kéo object từ Hierarchy vào.");
            return;
        }

        templateInstance = Instantiate(plateContainerTemplate);
        templateInstance.name = "PlateContainer_TEMPLATE";
        templateInstance.SetActive(false);
        DontDestroyOnLoad(templateInstance);

        if (PlayerPrefs.HasKey("LevelToLoad"))
        {
            currentLevel = PlayerPrefs.GetInt("LevelToLoad", 1);
        }
    }

    void Start()
    {
        if (templateInstance == null)
        {
            Debug.LogError("Template null, không thể chạy game!");
            return;
        }

        foodMap = new Dictionary<int, Sprite>();
        for (int i = 0; i < foodSprites.Length; i++)
        {
            if (foodSprites[i] != null)
                foodMap[i + 1] = foodSprites[i];
        }

        LoadLevel(currentLevel);

        if (!hasAutoReloaded && currentLevelData != null)
        {
            hasAutoReloaded = true;
            ClearAllPlateContainers();
            LoadLevel(currentLevel);

            if (scoreManager != null)
            {
                scoreManager.RefreshFromUI();
                scoreManager.ReplayGame();
            }
        }
    }

    void LoadLevel(int level)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/level_{level}");
        if (jsonFile == null)
        {
            Debug.LogError($"Không tìm thấy level_{level}");
            return;
        }

        currentLevelData = JsonUtility.FromJson<LevelData>(jsonFile.text);
        if (currentLevelData == null)
        {
            Debug.LogError("Parse JSON failed");
            return;
        }

        SetupTopbar(currentLevelData);
        SetupBoard(currentLevelData);
        SetupObstacles(currentLevelData);
    }

    void SetupTopbar(LevelData data)
    {
        levelText.text = $"Lv. {data.level}";
        timeText.text = FormatTime(data.time);
        stepText.text = $"0/{data.step}";

        if (scoreManager != null)
        {
            scoreManager.SetTotalSteps(data.step);
            if (!hasAutoReloaded)
            {
                scoreManager.RefreshFromUI();
                scoreManager.ResetGame();
            }
        }
    }

    string FormatTime(int totalSeconds)
    {
        int minute = totalSeconds / 60;
        int second = totalSeconds % 60;
        return $"{minute:00}:{second:00}";
    }

    void SetupBoard(LevelData data)
    {
        for (int row = 0; row < data.layout.Length; row++)
        {
            for (int col = 0; col < data.layout[row].row.Length; col++)
            {
                int trayId = data.layout[row].row[col];
                TrayData trayData = GetTrayData(data.trays, trayId);
                if (trayData == null) continue;

                if (row < trayRows.Count && col < trayRows[row].childCount)
                {
                    Transform tray = trayRows[row].GetChild(col);
                    SetupTray(tray, trayData);
                }
                else
                {
                    Debug.LogWarning($"Row {row} or Col {col} out of range!");
                }
            }
        }
    }

    // ---------------------------------------------------------------
    // OBSTACLE SETUP (CẬP NHẬT THÊM HARD LOCK)
    // ---------------------------------------------------------------

    void SetupObstacles(LevelData data)
    {
        // Clear previous FoodLock instances
        foreach (var go in activeFoodLocks.Values)
            if (go != null) Destroy(go);
        activeFoodLocks.Clear();

        // Clear previous HardLock instances // THÊM MỚI
        foreach (var go in activeHardLocks.Values)
            if (go != null) Destroy(go);
        activeHardLocks.Clear();

        if (data.obstacle == null || data.obstacle.Count == 0) return;

        foreach (ObstacleData obs in data.obstacle)
        {
            if (obs.type == "FoodLock")
            {
                SetupFoodLock(obs, data);
            }
            else if (obs.type == "HardLock") // THÊM MỚI
            {
                SetupHardLock(obs, data);
            }
        }
    }

    void SetupFoodLock(ObstacleData obs, LevelData data)
    {
        if (foodLockPrefab == null)
        {
            Debug.LogWarning("FoodLock Prefab chưa được gán vào LevelLoader!");
            return;
        }

        Transform trayTransform = GetTrayTransformByLayoutPosition(obs.layout, data);
        if (trayTransform == null)
        {
            Debug.LogWarning($"Không tìm thấy tray tại layout position {obs.layout}");
            return;
        }

        GameObject lockGO = Instantiate(foodLockPrefab, trayTransform);
        lockGO.name = "FoodLock";
        lockGO.SetActive(true);

        Sprite foodSprite = null;
        if (!string.IsNullOrEmpty(obs.FoodLockImg) &&
            int.TryParse(obs.FoodLockImg, out int foodId) &&
            foodMap.ContainsKey(foodId))
        {
            foodSprite = foodMap[foodId];
            SpriteRenderer sr = lockGO.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = foodSprite;
                sr.enabled = true;
            }
        }

        FoodLockController controller = lockGO.GetComponent<FoodLockController>();
        if (controller != null)
            controller.Init(obs.FoodLockImg, foodSprite);

        activeFoodLocks[obs.layout] = lockGO;
        Debug.Log($"FoodLock spawned at layout position {obs.layout}, img={obs.FoodLockImg}");
    }

    // THÊM MỚI: Setup Hard Lock
    void SetupHardLock(ObstacleData obs, LevelData data)
    {
        if (hardLockPrefab == null)
        {
            Debug.LogWarning("HardLock Prefab chưa được gán vào LevelLoader!");
            return;
        }

        Transform trayTransform = GetTrayTransformByLayoutPosition(obs.layout, data);
        if (trayTransform == null)
        {
            Debug.LogWarning($"Không tìm thấy tray tại layout position {obs.layout} cho HardLock");
            return;
        }

        GameObject hardLockGO = Instantiate(hardLockPrefab, trayTransform);
        hardLockGO.name = "HardLock";
        hardLockGO.SetActive(true);

        activeHardLocks[obs.layout] = hardLockGO;
        Debug.Log($"HardLock spawned at layout position {obs.layout}");
    }

    Transform GetTrayTransformByLayoutPosition(int layoutPosition, LevelData data)
    {
        int counter = 0;
        for (int row = 0; row < data.layout.Length; row++)
        {
            for (int col = 0; col < data.layout[row].row.Length; col++)
            {
                counter++;
                if (counter == layoutPosition)
                {
                    if (row < trayRows.Count && col < trayRows[row].childCount)
                        return trayRows[row].GetChild(col);
                    return null;
                }
            }
        }
        return null;
    }

    // ---------------------------------------------------------------
    // PUBLIC: Called by TrayManager when 3 matching foods are eaten
    // ---------------------------------------------------------------
    public void OnFoodEaten(string foodTypeId)
    {
        if (currentLevelData == null || currentLevelData.obstacle == null) return;

        foreach (ObstacleData obs in currentLevelData.obstacle)
        {
            if (obs.type != "FoodLock") continue;
            if (obs.FoodLockImg != foodTypeId) continue;

            if (activeFoodLocks.TryGetValue(obs.layout, out GameObject lockGO) && lockGO != null)
            {
                lockGO.SetActive(false);
                Debug.Log($"FoodLock tại layout {obs.layout} đã được mở khóa bởi food {foodTypeId}");
            }
        }
    }

    // THÊM MỚI: Public method để mở HardLock từ bên ngoài
    public void UnlockHardLock(int layoutPosition)
    {
        if (activeHardLocks.TryGetValue(layoutPosition, out GameObject hardLockGO) && hardLockGO != null)
        {
            hardLockGO.SetActive(false);
            Debug.Log($"HardLock tại layout {layoutPosition} đã được mở khóa!");
        }
    }

    // ---------------------------------------------------------------
    // Existing helpers
    // ---------------------------------------------------------------

    TrayData GetTrayData(TrayCollection trays, int key)
    {
        foreach (TrayItem item in trays.items)
        {
            if (item.key == key)
                return item.value;
        }
        return null;
    }

    void SetupTray(Transform tray, TrayData trayData)
    {
        if (templateInstance == null)
        {
            Debug.LogError($"Template null ở tray {tray.name}");
            return;
        }

        List<Transform> trayFoods = new List<Transform>();
        foreach (Transform child in tray)
        {
            if (child.name == "TrayFood")
                trayFoods.Add(child);
        }

        if (trayFoods.Count >= 3)
        {
            SetTrayFood(trayFoods[0], trayData.visible._1);
            SetTrayFood(trayFoods[1], trayData.visible._2);
            SetTrayFood(trayFoods[2], trayData.visible._3);
        }

        Transform plateCol = tray.Find("PlateCol");
        if (plateCol == null)
        {
            Debug.LogError($"Không tìm thấy PlateCol trong {tray.name}");
            return;
        }

        for (int i = plateCol.childCount - 1; i >= 0; i--)
            DestroyImmediate(plateCol.GetChild(i).gameObject);

        int containerIndex = 0;
        for (int i = trayData.hidden.Count - 1; i >= 0; i--)
        {
            GameObject newContainer = Instantiate(templateInstance, plateCol);
            newContainer.SetActive(true);
            newContainer.name = "PlateContainer_" + containerIndex;
            SetupPlate(newContainer.transform, trayData.hidden[i], containerIndex);
            containerIndex++;
        }
    }

    void SetTrayFood(Transform trayFood, string foodTypeId)
    {
        if (trayFood == null || trayFood.childCount == 0) return;
        SpriteRenderer sr = trayFood.GetChild(0).GetComponent<SpriteRenderer>();
        SetFoodSprite(sr, foodTypeId);
    }

    void SetupPlate(Transform plateContainer, VisibleFood hidden, int containerIndex)
    {
        int diaOrder = 10 + (containerIndex * 2);
        float posY = containerIndex * 0.1f;

        Vector3 containerPos = plateContainer.localPosition;
        containerPos.y = posY;
        plateContainer.localPosition = containerPos;

        SpriteRenderer[] allRenderers = plateContainer.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in allRenderers)
        {
            if (sr == null) continue;
            if (sr.name.Contains("Dia") || sr.name.Contains("Plate"))
                sr.sortingOrder = diaOrder;
            else
                sr.sortingOrder = diaOrder + 2;
        }

        List<Transform> plateFoods = new List<Transform>();
        foreach (Transform child in plateContainer.GetComponentsInChildren<Transform>(true))
        {
            if (child != null && child.name == "PlateFood")
                plateFoods.Add(child);
        }
        plateFoods.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        string[] foods = { hidden._1, hidden._2, hidden._3 };
        for (int i = 0; i < plateFoods.Count && i < foods.Length; i++)
        {
            Transform foodSlot = plateFoods[i];
            if (foodSlot == null || foodSlot.childCount == 0) continue;
            SpriteRenderer sr = foodSlot.GetChild(0).GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                SetFoodSprite(sr, foods[i]);
                sr.sortingOrder = diaOrder + 2;
            }
        }
    }

    void SetFoodSprite(SpriteRenderer sr, string foodTypeId)
    {
        if (sr == null) return;

        if (string.IsNullOrEmpty(foodTypeId))
        {
            sr.enabled = false;
            sr.sprite = null;
            return;
        }

        if (int.TryParse(foodTypeId, out int id) && foodMap.ContainsKey(id))
        {
            sr.enabled = true;
            sr.sprite = foodMap[id];
        }
        else
        {
            sr.enabled = false;
            sr.sprite = null;
        }
    }

    public void ReloadCurrentLevel()
    {
        if (isReloading) return;
        isReloading = true;

        if (templateInstance == null)
        {
            Debug.LogError("Template null! Không thể reload.");
            isReloading = false;
            return;
        }

        ClearAllPlateContainers();
        LoadLevel(currentLevel);

        if (scoreManager != null)
        {
            scoreManager.RefreshFromUI();
            scoreManager.ReplayGame();
        }

        isReloading = false;
    }

    private void ClearAllPlateContainers()
    {
        foreach (Transform trayRow in trayRows)
        {
            for (int i = 0; i < trayRow.childCount; i++)
            {
                Transform tray = trayRow.GetChild(i);
                Transform plateCol = tray.Find("PlateCol");
                if (plateCol != null)
                {
                    for (int j = plateCol.childCount - 1; j >= 0; j--)
                        DestroyImmediate(plateCol.GetChild(j).gameObject);
                }
            }
        }
    }

    public void LoadNextLevel()
    {
        currentLevel++;
        hasAutoReloaded = false;
        LoadLevel(currentLevel);

        if (!hasAutoReloaded && currentLevelData != null)
        {
            hasAutoReloaded = true;
            ClearAllPlateContainers();
            LoadLevel(currentLevel);
        }

        if (scoreManager != null)
        {
            scoreManager.RefreshFromUI();
            scoreManager.ResetGame();
        }
    }

    public LevelData GetCurrentLevelData()
    {
        return currentLevelData;
    }
}