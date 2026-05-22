using System.Collections.Generic;
using UnityEngine;

public class TrayManager : MonoBehaviour
{
    public List<TrayFoodSlot> traySlots = new();

    public void CheckTrayState()
    {
        if (traySlots.Count < 3)
            return;

        bool allEmpty = IsAllEmpty();
        bool allSame = IsAllSame();

        if (!allEmpty && !allSame)
            return;

        ClearTray();

        PullTopPlateContainer();

        // IMPORTANT: Add this line to update steps when food is eaten
        if (!allEmpty && allSame)
        {
            if (ScoreManager.Instance != null)  // Đã đổi từ TopBarController thành ScoreManager
            {
                ScoreManager.Instance.AddStep();  // Đã đổi từ TopBarController thành ScoreManager
            }
        }
    }

    bool IsAllEmpty()
    {
        foreach (var slot in traySlots)
        {
            if (!slot.IsEmpty())
                return false;
        }

        return true;
    }

    bool IsAllSame()
    {
        Sprite firstSprite = traySlots[0].GetSprite();

        if (firstSprite == null)
            return false;

        for (int i = 1; i < traySlots.Count; i++)
        {
            if (traySlots[i].GetSprite() != firstSprite)
                return false;
        }

        return true;
    }

    void ClearTray()
    {
        foreach (var slot in traySlots)
        {
            slot.SetSprite(null);
        }
    }

    void PullTopPlateContainer()
    {
        Transform plateCol = transform.Find("PlateCol");

        if (plateCol == null)
            return;

        if (plateCol.childCount == 0)
            return;

        Transform topContainer =
            plateCol.GetChild(plateCol.childCount - 1);

        List<Transform> plateFoods = new();

        foreach (Transform child in topContainer.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "PlateFood")
            {
                plateFoods.Add(child);
            }
        }

        plateFoods.Sort((a, b) =>
            a.localPosition.y.CompareTo(b.localPosition.y));

        for (int i = 0; i < traySlots.Count; i++)
        {
            Sprite sprite = null;

            if (i < plateFoods.Count &&
                plateFoods[i].childCount > 0)
            {
                SpriteRenderer sr =
                    plateFoods[i]
                    .GetChild(0)
                    .GetComponent<SpriteRenderer>();

                if (sr != null && sr.enabled)
                {
                    sprite = sr.sprite;
                }
            }

            traySlots[i].SetSprite(sprite);
        }

        Destroy(topContainer.gameObject);
    }
}