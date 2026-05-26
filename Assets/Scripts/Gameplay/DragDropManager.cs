using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DragDropManager : MonoBehaviour
{
    private Camera cam;

    private TrayFoodSlot currentSlot;

    private Vector3 startPosition;
    private Vector3 offset;

    private bool isDragging;
    private bool isClick;
    private float clickTimer;

    private SpriteRenderer spriteRenderer;
    private int originalSortingOrder;

    [Header("Snap")]
    public float snapRadius = 1.2f;

    [Header("Click & Drag Settings")]
    public float clickThreshold = 0.1f;
    public float selectedScale = 1.2f;
    private Vector3 originalScale;

    [Header("Popup Panel")]
    public string popupPanelName = "PausePopup";
    private GameObject popupPanel;
    public BoxCollider2D targetCollider;

    // Static variable để theo dõi item đang được chọn
    private static DragDropManager currentSelectedItem;

    // Sorting order khi đang kéo
    private const int DRAGGING_SORTING_ORDER = 1000;

    private void Awake()
    {
        cam = Camera.main;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        originalScale = transform.localScale;

        FindPopupPanel();
    }

    private void FindPopupPanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            popupPanel = canvas.transform.Find(popupPanelName)?.gameObject;

            if (popupPanel == null)
            {
                Transform[] allTransforms = canvas.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in allTransforms)
                {
                    if (t.name == popupPanelName)
                    {
                        popupPanel = t.gameObject;
                        break;
                    }
                }
            }
        }

        if (popupPanel == null)
        {
            popupPanel = GameObject.Find(popupPanelName);
        }

        if (popupPanel == null)
        {
            Debug.LogWarning($"Không tìm thấy Popup Panel tên: {popupPanelName}");
        }
        else
        {
            Debug.Log($"Đã tìm thấy Popup Panel: {popupPanelName} trong Canvas");
        }
    }

    private void Update()
    {
        if (popupPanel == null && !string.IsNullOrEmpty(popupPanelName))
        {
            FindPopupPanel();
        }

        if (popupPanel != null && targetCollider != null)
        {
            targetCollider.enabled = !popupPanel.activeSelf;
        }
    }

    private void OnMouseDown()
    {
        if (IsBlockedByFoodLock())
            return;

        if (popupPanel != null && popupPanel.activeSelf)
            return;

        if (currentSelectedItem != null && currentSelectedItem != this)
        {
            currentSelectedItem.ResetSelection();
        }

        currentSlot = GetComponent<TrayFoodSlot>();

        if (currentSlot == null) return;
        if (currentSlot.IsEmpty()) return;

        startPosition = transform.position;

        Vector3 mouseWorld = GetMouseWorldPosition();
        offset = transform.position - mouseWorld;

        isDragging = false;
        isClick = true;
        clickTimer = 0f;

        transform.localScale = originalScale * selectedScale;

        // Khi bắt đầu click (chưa kéo) - vẫn giữ sorting order 1000
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = DRAGGING_SORTING_ORDER;
        }

        currentSelectedItem = this;
    }

    private void OnMouseDrag()
    {
        if (popupPanel != null && popupPanel.activeSelf)
            return;

        if (currentSlot == null) return;

        clickTimer += Time.deltaTime;

        if (clickTimer >= clickThreshold && !isDragging)
        {
            isDragging = true;
            isClick = false;
        }

        if (isDragging)
        {
            Vector3 mouseWorld = GetMouseWorldPosition();
            transform.position = mouseWorld + offset;

            // Đảm bảo sorting order là 1000 trong suốt quá trình kéo
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = DRAGGING_SORTING_ORDER;
            }
        }
    }

    private void OnMouseUp()
    {
        if (popupPanel != null && popupPanel.activeSelf)
            return;

        if (currentSlot == null) return;

        if (isDragging)
        {
            TrayFoodSlot targetSlot = FindNearestValidSlot();

            if (targetSlot == null)
            {
                ReturnBack();
            }
            else
            {
                MoveToTarget(targetSlot);
            }
        }
        else if (isClick)
        {
            // Click ngắn - giữ nguyên scale to và sorting order 1000
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = DRAGGING_SORTING_ORDER;
            }
        }

        ResetDragState();
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

            float distance = Vector2.Distance(transform.position, slot.transform.position);

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
        Sprite movingSprite = currentSlot.GetSprite();

        currentSlot.SetSprite(null);
        targetSlot.SetSprite(movingSprite);

        TrayManager sourceTray = currentSlot.GetTray();
        TrayManager targetTray = targetSlot.GetTray();

        if (sourceTray != null)
            sourceTray.CheckTrayState();

        if (targetTray != null && targetTray != sourceTray)
            targetTray.CheckTrayState();

        transform.position = startPosition;
        ResetSortingOrder();
        ResetScale();
    }

    private void ReturnBack()
    {
        transform.position = startPosition;
        ResetSortingOrder();
        ResetScale();
    }

    private void ResetSortingOrder()
    {
        if (spriteRenderer != null)
            spriteRenderer.sortingOrder = originalSortingOrder;
    }

    private void ResetScale()
    {
        transform.localScale = originalScale;
    }

    private void ResetDragState()
    {
        isDragging = false;
        isClick = false;
        clickTimer = 0f;
        currentSlot = null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z);
        Vector3 world = cam.ScreenToWorldPoint(mousePos);
        world.z = 0f;
        return world;
    }

    public void ResetToOriginalPosition()
    {
        if (currentSlot != null)
        {
            startPosition = currentSlot.transform.position;
            transform.position = startPosition;
        }

        ResetSortingOrder();
        ResetScale();
        ResetDragState();
    }

    private void ResetSelection()
    {
        ResetScale();
        ResetSortingOrder();
        if (currentSelectedItem == this)
        {
            currentSelectedItem = null;
        }
    }

    private void OnDestroy()
    {
        if (currentSelectedItem == this)
        {
            currentSelectedItem = null;
        }
    }

    private bool IsBlockedByFoodLock()
    {
        Vector2 mousePos = GetMouseWorldPosition();

        Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);

        bool clickedFoodLock = false;
        bool clickedHardLock = false;
        bool clickedThisItem = false;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.gameObject.layer == LayerMask.NameToLayer("FoodLock"))
            {
                clickedFoodLock = true;
            }

            if (hit.gameObject.layer == LayerMask.NameToLayer("HardLock"))
            {
                clickedHardLock = true;
            }

            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                clickedThisItem = true;
            }
        }

        return (clickedFoodLock || clickedHardLock) && clickedThisItem;
    }
}