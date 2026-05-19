using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleSwitch : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public RectTransform handle;
    public Image background;

    [Header("Sprites")]
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Animation")]
    public float slideSpeed = 10f;

    private bool isOn = true;

    private float onX;
    private float offX;
    private float targetX;

    private void Start()
    {
        onX = Mathf.Abs(handle.anchoredPosition.x);
        offX = -onX;

        targetX = isOn ? onX : offX;

        UpdateVisual();
    }

    private void Update()
    {
        Vector2 currentPos = handle.anchoredPosition;

        currentPos.x = Mathf.Lerp(
            currentPos.x,
            targetX,
            slideSpeed * Time.deltaTime
        );

        handle.anchoredPosition = currentPos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isOn = !isOn;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        targetX = isOn ? onX : offX;

        if (background != null)
        {
            background.sprite = isOn ? onSprite : offSprite;
        }
    }
}