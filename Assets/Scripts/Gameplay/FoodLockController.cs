using UnityEngine;

public class FoodLockController : MonoBehaviour
{
    [Header("FoodLockImg")]
    public SpriteRenderer foodImageRenderer;

    public string requiredFoodId;

    // Trạng thái khóa
    private bool isLocked = true;

    public void Init(string foodId, Sprite foodSprite)
    {
        requiredFoodId = foodId;
        isLocked = true;

        if (foodImageRenderer != null)
        {
            foodImageRenderer.sprite = foodSprite;
            foodImageRenderer.enabled = true;
        }
        else
        {
            Debug.LogWarning("FoodLockController: foodImageRenderer chưa được gán! Hãy kéo SpriteRenderer vào Inspector.");
        }
    }

    public bool IsLocked()
    {
        return isLocked && gameObject.activeSelf;
    }

    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;
        gameObject.SetActive(false);
        Debug.Log($"FoodLock với requiredFoodId={requiredFoodId} đã được mở khóa!");
    }
    public string GetRequiredFoodId()
    {
        return requiredFoodId;
    }

    public void ResetLock()
    {
        isLocked = true;
        gameObject.SetActive(true);

        if (foodImageRenderer != null)
        {
            foodImageRenderer.enabled = true;
        }
    }
}