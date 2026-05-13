using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomNavController : MonoBehaviour
{
    [System.Serializable]
    public class NavItem
    {
        public Button button;

        public RectTransform icon;

        public TMP_Text label;

        public GameObject page;

        public LayoutElement layoutElement;

        [HideInInspector]
        public Coroutine animationCoroutine;
    }

    [Header("Navigation")]
    public NavItem[] navItems;

    [Header("Animation")]
    public float animationDuration = 0.25f;

    [Header("Button Width")]
    public float normalWidth = 120f;
    public float selectedWidth = 180f;

    [Header("Icon Scale")]
    public float normalScale = 1f;
    public float selectedScale = 1.6f;

    [Header("Icon Position")]
    public float normalY = 0f;
    public float selectedY = 40f;

    private int currentIndex = -1;

    private void Start()
    {
        for (int i = 0; i < navItems.Length; i++)
        {
            int index = i;

            navItems[i].button.onClick.AddListener(() =>
            {
                SelectTab(index);
            });

            navItems[i].layoutElement.preferredWidth = normalWidth;

            navItems[i].icon.localScale =
                Vector3.one * normalScale;

            navItems[i].icon.anchoredPosition =
                new Vector2(
                    navItems[i].icon.anchoredPosition.x,
                    normalY
                );

            navItems[i].label.gameObject.SetActive(false);

            navItems[i].page.SetActive(false);
        }

        SelectTab(2);
    }

    public void SelectTab(int index)
    {
        if (index == currentIndex)
        {
            return;
        }

        currentIndex = index;

        for (int i = 0; i < navItems.Length; i++)
        {
            bool isSelected = i == index;

            navItems[i].page.SetActive(isSelected);

            if (navItems[i].animationCoroutine != null)
            {
                StopCoroutine(
                    navItems[i].animationCoroutine
                );
            }

            navItems[i].animationCoroutine =
                StartCoroutine(
                    AnimateButton(
                        navItems[i],
                        isSelected
                    )
                );
        }
    }

    private IEnumerator AnimateButton(
        NavItem item,
        bool selected
    )
    {
        float targetWidth =
            selected
            ? selectedWidth
            : normalWidth;

        float targetScale =
            selected
            ? selectedScale
            : normalScale;

        float targetY =
            selected
            ? selectedY
            : normalY;

        float startWidth =
            item.layoutElement.preferredWidth;

        Vector3 startScale =
            item.icon.localScale;

        Vector2 startPos =
            item.icon.anchoredPosition;

        Vector3 endScale =
            Vector3.one * targetScale;

        Vector2 endPos =
            new Vector2(
                startPos.x,
                targetY
            );

        if (selected)
        {
            item.label.gameObject.SetActive(true);
        }

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                elapsed / animationDuration;

            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            item.layoutElement.preferredWidth =
                Mathf.Lerp(
                    startWidth,
                    targetWidth,
                    t
                );

            item.icon.localScale =
                Vector3.Lerp(
                    startScale,
                    endScale,
                    t
                );

            item.icon.anchoredPosition =
                Vector2.Lerp(
                    startPos,
                    endPos,
                    t
                );

            LayoutRebuilder
                .ForceRebuildLayoutImmediate(
                    transform as RectTransform
                );

            yield return null;
        }

        item.layoutElement.preferredWidth =
            targetWidth;

        item.icon.localScale =
            endScale;

        item.icon.anchoredPosition =
            endPos;

        if (!selected)
        {
            item.label.gameObject
                .SetActive(false);
        }
    }
}