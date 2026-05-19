using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelLoader : MonoBehaviour
{
    [Header("Tray")]
    public GameObject trayPrefab;

    [Header("Food")]
    public GameObject foodImagePrefab;

    [Header("Board Root")]
    public Transform boardRoot;

    [Header("Food Sprites")]
    public Sprite M;
    public Sprite S;
    public Sprite C;
    public Sprite B;
    public Sprite CK;
    public Sprite MS;
    public Sprite EP;

    [Header("UI Text")]
    public TMP_Text levelText;
    public TMP_Text timeText;
    public TMP_Text perfectText;

    private readonly Dictionary<string, Sprite> foodMap =
        new Dictionary<string, Sprite>();

    private readonly Dictionary<string, Transform> slotMap =
        new Dictionary<string, Transform>();

    private readonly List<GameObject> spawnedTrays =
        new List<GameObject>();

    private void Start()
    {
        SetupFoodMap();
        CacheBoardSlots();
        LoadLevel(1);
    }

    private void SetupFoodMap()
    {
        foodMap.Clear();

        foodMap["M"] = M;
        foodMap["S"] = S;
        foodMap["C"] = C;
        foodMap["B"] = B;
        foodMap["CK"] = CK;
        foodMap["MS"] = MS;
        foodMap["EP"] = EP;
    }

    private void CacheBoardSlots()
    {
        slotMap.Clear();

        foreach (Transform child in boardRoot)
        {
            string key =
                child.name.Replace(
                    "_Position",
                    ""
                );

            slotMap[key] = child;
        }
    }

    public void LoadLevel(int level)
    {
        ClearBoard();

        TextAsset json =
            Resources.Load<TextAsset>(
                $"Levels/level_{level}"
            );

        if (json == null)
        {
            Debug.LogError(
                $"Không tìm thấy level_{level}.json"
            );

            return;
        }

        LevelData levelData =
            JsonConvert.DeserializeObject<LevelData>(
                json.text
            );

        if (levelData == null)
        {
            Debug.LogError(
                "Parse JSON thất bại"
            );

            return;
        }

        // Cập nhật UI với dữ liệu từ JSON
        UpdateUI(levelData);

        HashSet<string> validTrays =
            new HashSet<string>();

        foreach (string[] row in levelData.layout)
        {
            foreach (string trayKey in row)
            {
                if (trayKey == "X")
                {
                    continue;
                }

                validTrays.Add(trayKey);
            }
        }

        foreach (
            KeyValuePair<string, TrayData> trayPair
            in levelData.trays
        )
        {
            string trayKey =
                trayPair.Key;

            TrayData trayData =
                trayPair.Value;

            if (!validTrays.Contains(trayKey))
            {
                continue;
            }

            if (!slotMap.ContainsKey(trayKey))
            {
                Debug.LogWarning(
                    $"Không có position cho {trayKey}"
                );

                continue;
            }

            Transform spawnPoint =
                slotMap[trayKey];

            GameObject tray =
                Instantiate(
                    trayPrefab,
                    spawnPoint
                );

            tray.transform.localPosition =
                Vector3.zero;

            tray.transform.localRotation =
                Quaternion.identity;

            tray.transform.localScale =
                Vector3.one;

            TrayView trayView =
                tray.GetComponent<TrayView>();

            if (trayView == null)
            {
                Debug.LogError(
                    "TrayPrefab thiếu TrayView"
                );

                continue;
            }

            SetupVisible(
                trayView,
                trayData.visible
            );

            SetupHidden(
                trayView,
                trayData.hidden
            );

            spawnedTrays.Add(tray);
        }

        Debug.Log(
            $"Load level {level} thành công"
        );
    }

    /// <summary>
    /// Cập nhật UI Text với dữ liệu từ level JSON
    /// </summary>
    private void UpdateUI(LevelData levelData)
    {
        // Hiển thị Level: "Lv. 1"
        if (levelText != null)
        {
            levelText.text = $"Lv. {levelData.level}";
        }

        // Hiển thị thời gian: "05:00" (định dạng phút:giây)
        if (timeText != null)
        {
            int minutes = levelData.time / 60;
            int seconds = levelData.time % 60;
            timeText.text = $"{minutes:00}:{seconds:00}";
        }

        // Hiển thị Perfect: "0/10" (số sao đạt được / tổng perfect)
        if (perfectText != null)
        {
            // currentPerfect là 0 ban đầu, bạn có thể cập nhật sau khi người chơi hoàn thành
            int currentPerfect = 0;
            perfectText.text = $"{currentPerfect}/{levelData.perfect}";
        }
    }

    /// <summary>
    /// Gọi hàm này khi người chơi đạt được perfect mới
    /// </summary>
    public void UpdatePerfect(int currentPerfect, int maxPerfect)
    {
        if (perfectText != null)
        {
            perfectText.text = $"{currentPerfect}/{maxPerfect}";
        }
    }

    private void SetupVisible(
        TrayView trayView,
        SlotData slotData
    )
    {
        if (slotData == null)
        {
            return;
        }

        SpawnFood(
            slotData.slot1,
            trayView.GetFoodSlot(1)
        );

        SpawnFood(
            slotData.slot2,
            trayView.GetFoodSlot(2)
        );

        SpawnFood(
            slotData.slot3,
            trayView.GetFoodSlot(3)
        );
    }

    private void SetupHidden(
        TrayView trayView,
        List<SlotData> hidden
    )
    {
        for (
            int i = 0;
            i < trayView.hiddenPlates.Length;
            i++
        )
        {
            trayView.hiddenPlates[i]
                .gameObject
                .SetActive(false);

            ClearChildren(
                trayView.GetHiddenFoodSlot(i, 1)
            );

            ClearChildren(
                trayView.GetHiddenFoodSlot(i, 2)
            );

            ClearChildren(
                trayView.GetHiddenFoodSlot(i, 3)
            );
        }

        if (
            hidden == null
            || hidden.Count == 0
        )
        {
            return;
        }

        int total =
            hidden.Count;

        for (
            int i = 0;
            i < total;
            i++
        )
        {
            int plateIndex =
                total - 1 - i;

            Transform plate =
                trayView.hiddenPlates[
                    plateIndex
                ];

            plate.gameObject.SetActive(
                true
            );

            SlotData data =
                hidden[i];

            SpawnFood(
                data.slot1,
                trayView.GetHiddenFoodSlot(
                    plateIndex,
                    1
                )
            );

            SpawnFood(
                data.slot2,
                trayView.GetHiddenFoodSlot(
                    plateIndex,
                    2
                )
            );

            SpawnFood(
                data.slot3,
                trayView.GetHiddenFoodSlot(
                    plateIndex,
                    3
                )
            );
        }
    }

    private void SpawnFood(
    string foodKey,
    Transform parent
)
    {
        if (parent == null)
        {
            return;
        }

        Image image =
            parent.GetComponent<Image>();

        if (image == null)
        {
            Debug.LogWarning(
                $"Không có Image ở {parent.name}"
            );

            return;
        }

        if (
            string.IsNullOrEmpty(
                foodKey
            )
        )
        {
            image.sprite =
                null;

            image.color =
                new Color(
                    1f,
                    1f,
                    1f,
                    0f
                );

            return;
        }

        if (
            !foodMap.ContainsKey(
                foodKey
            )
        )
        {
            Debug.LogWarning(
                $"Không có sprite {foodKey}"
            );

            image.sprite =
                null;

            return;
        }

        image.sprite =
            foodMap[
                foodKey
            ];

        image.color =
            Color.white;

        image.preserveAspect =
            true;
    }

    private void ClearChildren(
        Transform parent
    )
    {
        if (parent == null)
        {
            return;
        }

        for (
            int i =
                parent.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                parent.GetChild(i)
                    .gameObject
            );
        }
    }

    private void ClearBoard()
    {
        foreach (
            GameObject tray
            in spawnedTrays
        )
        {
            if (tray != null)
            {
                Destroy(tray);
            }
        }

        spawnedTrays.Clear();
    }
}