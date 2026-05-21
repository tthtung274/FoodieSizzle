using UnityEngine;

public class TrayFoodSlot : MonoBehaviour
{
    public SpriteRenderer foodRenderer;

    private TrayManager trayManager;

    private void Awake()
    {
        if (foodRenderer == null)
        {
            foodRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        trayManager =
            GetComponentInParent<TrayManager>();
    }

    public TrayManager GetTray()
    {
        return trayManager;
    }

    public bool IsEmpty()
    {
        return foodRenderer == null ||
               !foodRenderer.enabled ||
               foodRenderer.sprite == null;
    }

    public Sprite GetSprite()
    {
        if (foodRenderer == null)
            return null;

        return foodRenderer.sprite;
    }

    public void SetSprite(Sprite sprite)
    {
        if (foodRenderer == null)
            return;

        if (sprite == null)
        {
            foodRenderer.sprite = null;
            foodRenderer.enabled = false;
        }
        else
        {
            foodRenderer.sprite = sprite;
            foodRenderer.enabled = true;
        }
    }
}