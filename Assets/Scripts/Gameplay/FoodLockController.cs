using UnityEngine;

/// <summary>
/// Gắn vào FoodLock prefab.
/// Kéo đúng SpriteRenderer của slot food (cái cà chua / ảnh food) vào foodImageRenderer.
/// LevelLoader sẽ gọi Init() để set sprite đúng theo FoodLockImg.
/// </summary>
public class FoodLockController : MonoBehaviour
{
    [Header("FoodLockImg")]
    public SpriteRenderer foodImageRenderer;

    // Food type ID cần unlock (set bởi LevelLoader)
    public string requiredFoodId;

    // Trạng thái khóa
    private bool isLocked = true;

    /// <summary>
    /// Gọi bởi LevelLoader sau khi spawn prefab.
    /// </summary>
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

    /// <summary>
    /// Kiểm tra FoodLock còn đang khóa hay không
    /// </summary>
    public bool IsLocked()
    {
        return isLocked && gameObject.activeSelf;
    }

    /// <summary>
    /// Mở khóa FoodLock (gọi từ LevelLoader khi ăn đúng food)
    /// </summary>
    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;
        gameObject.SetActive(false);
        Debug.Log($"FoodLock với requiredFoodId={requiredFoodId} đã được mở khóa!");
    }

    /// <summary>
    /// Lấy ID thức ăn cần để mở khóa
    /// </summary>
    public string GetRequiredFoodId()
    {
        return requiredFoodId;
    }

    /// <summary>
    /// Reset trạng thái (nếu cần reload level)
    /// </summary>
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