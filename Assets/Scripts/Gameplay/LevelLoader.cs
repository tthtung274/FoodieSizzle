using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

    [Header("Plate Container Prefab")]
    public GameObject plateContainerPrefab;

    private Dictionary<int, Sprite> foodMap;

    void Start()
    {
        if (plateContainerPrefab == null)
        {
            Debug.LogError("Chưa kéo thả PlateContainerPrefab!");
            return;
        }

        foodMap = new Dictionary<int, Sprite>();
        for (int i = 0; i < foodSprites.Length; i++)
        {
            if (foodSprites[i] != null)
                foodMap[i + 1] = foodSprites[i];
        }

        LoadLevel(currentLevel);
    }

    void LoadLevel(int level)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/level_{level}");
        if (jsonFile == null)
        {
            Debug.LogError($"Không tìm thấy level_{level}");
            return;
        }

        LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
        if (data == null)
        {
            Debug.LogError("Parse JSON failed");
            return;
        }

        SetupTopbar(data);
        SetupBoard(data);
    }

    void SetupTopbar(LevelData data)
    {
        levelText.text = $"Lv. {data.level}";
        timeText.text = FormatTime(data.time);
        stepText.text = $"0/{data.step}";
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

                Transform tray = trayRows[row].GetChild(col);
                SetupTray(tray, trayData);
            }
        }
    }

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
        // Gán visible foods
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

        // Xóa các container cũ
        for (int i = plateCol.childCount - 1; i >= 0; i--)
        {
            Destroy(plateCol.GetChild(i).gameObject);
        }

        // Tạo mới từng container với index tăng dần
        int containerIndex = 0;
        foreach (VisibleFood hidden in trayData.hidden)
        {
            GameObject newContainer = Instantiate(plateContainerPrefab, plateCol);
            SetupPlate(newContainer.transform, hidden, containerIndex);
            containerIndex++;
        }

        Debug.Log($"Tray {tray.name}: tạo {containerIndex} plate container");
    }

    void SetTrayFood(Transform trayFood, string foodTypeId)
    {
        if (trayFood == null || trayFood.childCount == 0) return;
        SpriteRenderer sr = trayFood.GetChild(0).GetComponent<SpriteRenderer>();
        SetFoodSprite(sr, foodTypeId);
    }

    void SetupPlate(Transform plateContainer, VisibleFood hidden, int containerIndex)
    {
        // Tính toán sorting order và position Y dựa trên index
        // index 0: order=10, posY=0
        // index 1: order=12, posY=0.1
        // index 2: order=14, posY=0.2
        // index 3: order=16, posY=0.3
        // index 4: order=18, posY=0.4

        int diaOrder = 10 + (containerIndex * 2);
        float posY = containerIndex * 0.1f;

        // Set position cho container
        Vector3 containerPos = plateContainer.localPosition;
        containerPos.y = posY;
        plateContainer.localPosition = containerPos;

        // Set sorting order cho tất cả SpriteRenderer
        SpriteRenderer[] allRenderers = plateContainer.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in allRenderers)
        {
            if (sr == null) continue;

            // Nếu là đĩa (Dia/Plate)
            if (sr.name.Contains("Dia") || sr.name.Contains("Plate"))
            {
                sr.sortingOrder = diaOrder;
            }
            else // Thức ăn
            {
                sr.sortingOrder = diaOrder + 2;
            }
        }

        // Lấy danh sách PlateFood và sắp xếp theo Y từ dưới lên
        List<Transform> plateFoods = new List<Transform>();
        foreach (Transform child in plateContainer.GetComponentsInChildren<Transform>(true))
        {
            if (child != null && child.name == "PlateFood")
                plateFoods.Add(child);
        }
        plateFoods.Sort((a, b) => a.localPosition.y.CompareTo(b.localPosition.y));

        // Gán món
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

        Debug.Log($"Container index {containerIndex}: Dia Order = {diaOrder}, Pos Y = {posY}");
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
}