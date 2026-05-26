using UnityEngine;
using UnityEngine.UI;

public class HardLockController : MonoBehaviour
{
    [Header("Lock Sprite")]
    public SpriteRenderer lockSprite;

    [Header("Panel and Button")]
    public GameObject unlockPopupPanel;
    public Button adsButton;

    private bool isLocked = true;

    void Start()
    {
        Debug.Log("HardLock Start");

        // Nếu chưa kéo trong Inspector thì mới tự tìm
        if (unlockPopupPanel == null)
        {
            unlockPopupPanel = FindDeepChild("UnlockPopup");
        }

        if (unlockPopupPanel == null)
        {
            Debug.LogError("Không tìm thấy UnlockPopup!");
            return;
        }

        // Nếu chưa kéo button thì mới tự tìm
        if (adsButton == null)
            adsButton = unlockPopupPanel.transform.Find("Ads")?.GetComponent<Button>();

        // Gán sự kiện cho nút Ads
        if (adsButton != null)
        {
            adsButton.onClick.RemoveAllListeners();
            adsButton.onClick.AddListener(() => {
                Debug.Log("ADS BUTTON CLICKED!");
                OnAdsUnlock();
            });
        }

        unlockPopupPanel.SetActive(false);
    }

    GameObject FindDeepChild(string name)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name)
                return obj;
        }
        return null;
    }

    void OnMouseDown()
    {
        Debug.Log("Click vào HardLock");
        if (isLocked && unlockPopupPanel != null)
        {
            unlockPopupPanel.SetActive(true);
        }
    }

    void OnAdsUnlock()
    {
        StartCoroutine(WatchAd());
    }

    System.Collections.IEnumerator WatchAd()
    {
        if (unlockPopupPanel != null) unlockPopupPanel.SetActive(false);
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
        isLocked = false;
    }

    public bool IsLocked() => isLocked;
}