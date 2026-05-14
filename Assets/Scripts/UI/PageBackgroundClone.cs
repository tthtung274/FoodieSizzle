using UnityEngine;
using UnityEngine.UI;

public class PageBackgroundClone : MonoBehaviour
{
    [Header("Home Background")]
    public Transform homeBackground;

    [Header("Target Background")]
    public Transform targetBackground;

    private GameObject currentBackground;

    private void OnEnable ()
    {
        RefreshBackground();
    }

    public void RefreshBackground()
    {
        if (currentBackground != null)
        {
            Destroy(currentBackground);
        }

        currentBackground = Instantiate(
            homeBackground.gameObject,
            targetBackground
        );

        currentBackground.transform.SetSiblingIndex(0);
    }
}