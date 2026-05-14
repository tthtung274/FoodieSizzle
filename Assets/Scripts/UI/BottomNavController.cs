using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BottomNavController : MonoBehaviour
{
    // Class Luu thong tin cac nut Nav Items
    [System.Serializable]
    public class NavItem
    {
        public Button button;
        public RectTransform icon;
        public TMP_Text label;
        public GameObject page;
        public LayoutElement layoutElement;

        [HideInInspector]
        public Coroutine animation;
    }

    // Khai bao bien
    [Header("Navigation")]
    public NavItem[] navItems;

    [Header("Animation")]
    public float animationDuration = 0.25f;

    [Header("Button")]
    public float normalButton = 160f;
    public float selectedButton = 220f;

    [Header("Icon")]
    public float normalIcon = 1f;
    public float selectedIcon = 1.6f;

    [Header("Icon Position")]
    public float normalPos = 0f;
    public float selectedPos = 40f;

    private int currentIndex = -1;

    // Ham khoi tao
    private void Start()
    {
        for (int i = 0; i < navItems.Length; i++)
        {
            int index = i;

            navItems[i].button.onClick.AddListener(() => SelectTab(index));

            navItems[i].layoutElement.preferredWidth = normalButton;
            navItems[i].icon.localScale = Vector3.one * normalIcon;
            navItems[i].icon.anchoredPosition = new Vector2(
                navItems[i].icon.anchoredPosition.x,
                normalPos
            );
            navItems[i].label.gameObject.SetActive(false);
            navItems[i].page.SetActive(false);
        }

        SelectTab(2);
    }

    // Xu ly khi Select Tab
    public void SelectTab(int index)
    {
        if (index == currentIndex)
            return;

        currentIndex = index;

        for (int i = 0; i < navItems.Length; i++)
        {
            bool isSelected = (i == index);

            navItems[i].page.SetActive(isSelected);

            if (navItems[i].animation != null)
            {
                StopCoroutine(navItems[i].animation);
            }

            navItems[i].animation = StartCoroutine(
                AnimateButton(navItems[i], isSelected)
            );
        }
    }

    // Ham Animation
    private IEnumerator AnimateButton(NavItem item, bool selected)
    {
        // 1. Xac dinh gia tri dich
        float targetButton = selected ? selectedButton : normalButton;
        float targetIcon = selected ? selectedIcon : normalIcon;
        float targetPos = selected ? selectedPos : normalPos;

        // 2. Gia tri bat dau va ket thuc
        float startButton = item.layoutElement.preferredWidth;
        Vector3 startIcon = item.icon.localScale;
        Vector2 startPos = item.icon.anchoredPosition;

        Vector3 endIcon = Vector3.one * targetIcon;
        Vector2 endPos = new Vector2(startPos.x, targetPos);

        // 3. Hien label
        if (selected)
        {
            item.label.gameObject.SetActive(true);
        }

        // 4. Run Animation
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / animationDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            item.layoutElement.preferredWidth = Mathf.Lerp(startButton, targetButton, t);
            item.icon.localScale = Vector3.Lerp(startIcon, endIcon, t);
            item.icon.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            yield return null;
        }
        // 5. Dam bao gia tri cuoi cung chinh xac
        item.layoutElement.preferredWidth = targetButton;
        item.icon.localScale = endIcon;
        item.icon.anchoredPosition = endPos;

        // 6. An label
        if (!selected)
        {
            item.label.gameObject.SetActive(false);
        }
    }
}