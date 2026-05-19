using UnityEngine;

public class TrayView : MonoBehaviour
{
    [Header("Visible")]
    public RectTransform foodSlot1;
    public RectTransform foodSlot2;
    public RectTransform foodSlot3;

    [Header("Hidden")]
    public RectTransform[] hiddenPlates;

    public RectTransform GetFoodSlot(
        int index
    )
    {
        switch (index)
        {
            case 1:
                return foodSlot1;

            case 2:
                return foodSlot2;

            case 3:
                return foodSlot3;
        }

        return null;
    }

    public RectTransform
        GetHiddenFoodSlot(
            int plateIndex,
            int slotIndex
        )
    {
        RectTransform plate =
            hiddenPlates[
                plateIndex
            ];

        return plate.GetChild(
            slotIndex - 1
        ) as RectTransform;
    }
}