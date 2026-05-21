using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [Header("Popup")]
    public GameObject popupSetting;
    public GameObject popupProfile;
    public GameObject popupPause;

    public void OpenPopupSetting()
    {
        if (popupSetting != null)
        {
            popupSetting.SetActive(true);
        }
    }

    public void ClosePopupSetting()
    {
        if (popupSetting != null)
        {
            popupSetting.SetActive(false);
        }
    }

    public void OpenPopupProfile()
    {
        if (popupProfile != null)
        {
            popupProfile.SetActive(true);
        }
    }

    public void ClosePopupProfile()
    {
        if (popupProfile != null)
        {
            popupProfile.SetActive(false);
        }
    }

    public void OpenPopupPause()
    {
        if (popupPause != null)
        {
            popupPause.SetActive(true);
        }
    }

    public void ClosePopupPause()
    {
        if (popupPause != null)
        {
            popupPause.SetActive(false);
        }
    }
}
