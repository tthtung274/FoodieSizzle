using UnityEngine;
using UnityEngine.EventSystems; 

public class ButtonClickEffect : MonoBehaviour, 
    IPointerDownHandler, 
    IPointerUpHandler, 
    IPointerExitHandler
{
    [Header("Scale Setting")]
    [SerializeField]
    private float pressedScale = 0.9f;
    [SerializeField]
    private float animationSpeed = 15f;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            animationSpeed * Time.deltaTime
        );
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = originalScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }
}
