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
    public string popupPanelName = "PausePopup"; // Tên của Panel
    private GameObject popupPanel;
    public BoxCollider2D targetCollider;

    private void Awake()
    {
        cam = Camera.main;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }

        originalScale = transform.localScale;

        // Tìm UI Panel trong Canvas
        FindPopupPanel();
    }

    private void FindPopupPanel()
    {
        // Tìm Canvas trước
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            // Tìm Panel theo tên trong Canvas
            popupPanel = canvas.transform.Find(popupPanelName)?.gameObject;

            if (popupPanel == null)
            {
                // Nếu không tìm thấy ở root, tìm deep trong Canvas
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
            // Thử tìm toàn bộ scene
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
        // Nếu chưa có popupPanel thì thử tìm lại
        if (popupPanel == null && !string.IsNullOrEmpty(popupPanelName))
        {
            FindPopupPanel();
        }

        // Xử lý popup: nếu panel active thì tắt collider
        if (popupPanel != null && targetCollider != null)
        {
            targetCollider.enabled = !popupPanel.activeSelf;
        }
    }

    private void OnMouseDown()
    {
        // Kiểm tra popup đang bật không cho tương tác
        if (popupPanel != null && popupPanel.activeSelf)
            return;

        // Debug.Log($"=== OnMouseDown called on {gameObject.name} ===");

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

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 15;
        }
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
            // Click ngắn - giữ nguyên scale to
            // transform.localScale = originalScale * selectedScale; // Giữ nguyên
        }

        ResetDragState();
    }

    // Các method khác giữ nguyên: FindNearestValidSlot, MoveToTarget, ReturnBack, ResetSortingOrder, ResetScale, ResetDragState, GetMouseWorldPosition
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
        // Reset về vị trí ban đầu
        if (currentSlot != null)
        {
            startPosition = currentSlot.transform.position;
            transform.position = startPosition;
        }

        ResetSortingOrder();
        ResetScale();
        ResetDragState();
    }
}