using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    [Header("UI")]
    public RectTransform loadingFill;
    public TMP_Text percentText;

    [Header("Animation")]
    public float fakeLoadingTime = 1.5f;

    private float maxWidth;

    private void Start()
    {
        maxWidth = loadingFill.sizeDelta.x;

        loadingFill.sizeDelta =
            new Vector2(0, loadingFill.sizeDelta.y);

        percentText.text = "0%";

        StartCoroutine(LoadSceneRoutine());
    }

    IEnumerator LoadSceneRoutine()
    {
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                LoadingManager.targetSceneName
            );

        operation.allowSceneActivation = false;

        float timer = 0f;

        while (timer < fakeLoadingTime)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(timer / fakeLoadingTime);

            float width =
                maxWidth * progress;

            loadingFill.sizeDelta =
                new Vector2(
                    width,
                    loadingFill.sizeDelta.y
                );

            percentText.text =
                Mathf.RoundToInt(progress * 100f) + "%";

            yield return null;
        }

        loadingFill.sizeDelta =
            new Vector2(
                maxWidth,
                loadingFill.sizeDelta.y
            );

        percentText.text = "100%";

        yield return new WaitForSeconds(0.3f);

        operation.allowSceneActivation = true;
    }
}