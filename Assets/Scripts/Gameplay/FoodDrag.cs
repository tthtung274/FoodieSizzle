// Assets/Scripts/UI/FoodDrag.cs

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodDrag : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private Vector2 startPosition;
    private Transform startParent;
    private int startSiblingIndex;

    private Transform dragRoot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvas = GetComponentInParent<Canvas>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                gameObject.AddComponent<CanvasGroup>();
        }

        GameObject root =
            GameObject.Find("DragRoot");

        if (root == null)
        {
            root = new GameObject("DragRoot");

            root.transform.SetParent(
                canvas.transform,
                false
            );

            RectTransform rt =
                root.AddComponent<RectTransform>();

            rt.anchorMin =
                Vector2.zero;

            rt.anchorMax =
                Vector2.one;

            rt.offsetMin =
                Vector2.zero;

            rt.offsetMax =
                Vector2.zero;
        }

        dragRoot = root.transform;
    }

    public void OnBeginDrag(
        PointerEventData eventData
    )
    {
        startPosition =
            rectTransform.anchoredPosition;

        startParent =
            transform.parent;

        startSiblingIndex =
            transform.GetSiblingIndex();

        transform.SetParent(
            dragRoot,
            true
        );

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts =
            false;
    }

    public void OnDrag(
        PointerEventData eventData
    )
    {
        rectTransform.anchoredPosition +=
            eventData.delta /
            canvas.scaleFactor;
    }

    public void OnEndDrag(
        PointerEventData eventData
    )
    {
        canvasGroup.blocksRaycasts =
            true;

        GameObject nearestSlot =
            FindNearestSlot();

        if (nearestSlot == null)
        {
            ReturnToStart();
            return;
        }

        RectTransform targetRect =
            nearestSlot.GetComponent<RectTransform>();

        if (targetRect == rectTransform)
        {
            ReturnToStart();
            return;
        }

        Vector2 targetPos =
            targetRect.anchoredPosition;

        Transform targetParent =
            targetRect.parent;

        int targetIndex =
            targetRect.GetSiblingIndex();

        targetRect.SetParent(
            startParent,
            false
        );

        targetRect.SetSiblingIndex(
            startSiblingIndex
        );

        targetRect.anchoredPosition =
            startPosition;

        transform.SetParent(
            targetParent,
            false
        );

        transform.SetSiblingIndex(
            targetIndex
        );

        rectTransform.anchoredPosition =
            targetPos;
    }

    private void ReturnToStart()
    {
        transform.SetParent(
            startParent,
            false
        );

        transform.SetSiblingIndex(
            startSiblingIndex
        );

        rectTransform.anchoredPosition =
            startPosition;
    }

    private GameObject FindNearestSlot()
    {
        FoodDrag[] allSlots =
            FindObjectsByType<FoodDrag>(
                FindObjectsSortMode.None
            );

        GameObject nearest =
            null;

        float minDistance =
            Mathf.Infinity;

        Vector2 mousePos =
            Input.mousePosition;

        foreach (FoodDrag slot
            in allSlots)
        {
            if (slot.gameObject ==
                gameObject)
            {
                continue;
            }

            RectTransform rt =
                slot.GetComponent
                <RectTransform>();

            Vector2 screenPos =
                RectTransformUtility
                .WorldToScreenPoint(
                    null,
                    rt.position
                );

            float distance =
                Vector2.Distance(
                    mousePos,
                    screenPos
                );

            if (distance <
                minDistance)
            {
                minDistance =
                    distance;

                nearest =
                    slot.gameObject;
            }
        }

        if (minDistance > 150f)
        {
            return null;
        }

        return nearest;
    }
}