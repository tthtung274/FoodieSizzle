using UnityEngine;

public class DragDropManager : MonoBehaviour
{
    private Camera cam;

    private TrayFoodSlot currentSlot;

    private Vector3 startPosition;
    private Vector3 offset;

    private bool isDragging;

    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    [Header("Snap")]
    public float snapRadius = 1.2f;

    private void Awake()
    {
        cam = Camera.main;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }

    private void OnMouseDown()
    {
        currentSlot = GetComponent<TrayFoodSlot>();

        if (currentSlot == null) return;
        if (currentSlot.IsEmpty()) return;

        startPosition = transform.position;

        Vector3 mouseWorld = GetMouseWorldPosition();
        offset = transform.position - mouseWorld;

        isDragging = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 15;
        }
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorld = GetMouseWorldPosition();
        transform.position = mouseWorld + offset;
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        TrayFoodSlot targetSlot = FindNearestValidSlot();

        if (targetSlot == null)
        {
            ReturnBack();
            return;
        }

        MoveToTarget(targetSlot);
    }

    private TrayFoodSlot FindNearestValidSlot()
    {
        TrayFoodSlot[] allSlots = FindObjectsOfType<TrayFoodSlot>();

        TrayFoodSlot nearestSlot = null;
        float nearestDistance = float.MaxValue;

        foreach (TrayFoodSlot slot in allSlots)
        {
            if (slot == currentSlot)
                continue;

            if (!slot.IsEmpty())
                continue;

            float distance = Vector2.Distance(
                transform.position,
                slot.transform.position
            );

            if (distance > snapRadius)
                continue;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestSlot = slot;
            }
        }

        return nearestSlot;
    }

    private void MoveToTarget(TrayFoodSlot targetSlot)
    {
        Sprite movingSprite =
            currentSlot.GetSprite();

        currentSlot.SetSprite(null);
        targetSlot.SetSprite(movingSprite);

        TrayManager sourceTray =
            currentSlot.GetTray();

        TrayManager targetTray =
            targetSlot.GetTray();

        if (sourceTray != null)
        {
            sourceTray.CheckTrayState();
        }

        if (targetTray != null &&
            targetTray != sourceTray)
        {
            targetTray.CheckTrayState();
        }

        transform.position = startPosition;

        ResetSortingOrder();
    }

    private void ReturnBack()
    {
        transform.position = startPosition;

        ResetSortingOrder();
    }

    private void ResetSortingOrder()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(mousePos);
        world.z = 0f;

        return world;
    }
}