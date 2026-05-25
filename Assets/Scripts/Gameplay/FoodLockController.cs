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

    /// <summary>
    /// Gọi bởi LevelLoader sau khi spawn prefab.
    /// </summary>
    public void Init(string foodId, Sprite foodSprite)
    {
        requiredFoodId = foodId;

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
}