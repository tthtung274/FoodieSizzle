using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text timeText;
    public TMP_Text stepText;

    [Header("Popup Panels")]
    public GameObject winPanel;
    public GameObject defeatPanel;

    private int currentSteps = 0;
    private int totalSteps = 0;
    private int currentTime = 0;
    private int totalTime = 0;
    private bool isGameActive = false;
    private bool isGameFinished = false;
    private bool isGamePaused = false;
    private Coroutine timerCoroutine;
    private DragDropManager dragDropManager;

    public System.Action<int> OnStepUpdated;
    public System.Action OnGameWin;
    public System.Action OnGameDefeat;

    public static ScoreManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        dragDropManager = FindObjectOfType<DragDropManager>();
    }

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        LoadDataFromUI();
    }

    private void LoadDataFromUI()
    {
        if (stepText != null)
        {
            string stepString = stepText.text;
            string[] parts = stepString.Split('/');
            if (parts.Length == 2)
            {
                currentSteps = int.Parse(parts[0]);
                totalSteps = int.Parse(parts[1]);
            }
        }

        if (timeText != null)
        {
            string timeString = timeText.text;
            string[] parts = timeString.Split(':');
            if (parts.Length == 2)
            {
                int minutes = int.Parse(parts[0]);
                int seconds = int.Parse(parts[1]);
                currentTime = minutes * 60 + seconds;
                totalTime = currentTime;
            }
        }

        Debug.Log($"Loaded from UI: Steps={currentSteps}/{totalSteps}, Time={currentTime}s");

        isGameActive = false;
        isGameFinished = false;
        isGamePaused = false;
    }

    public void RefreshFromUI()
    {
        LoadDataFromUI();

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        Debug.Log($"Refreshed: Steps={currentSteps}/{totalSteps}, Time={currentTime}s");
    }

    private void Update()
    {
        if (!isGameActive && !isGameFinished && !isGamePaused && dragDropManager != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartGame();
            }
        }
    }

    public void StartGame()
    {
        if (isGameFinished) return;
        if (isGameActive) return;
        if (isGamePaused) return;

        LoadDataFromUI();

        isGameActive = true;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(CountdownTimer());

        Debug.Log($"Game started! Time: {currentTime}s, Steps: {currentSteps}/{totalSteps}");
    }

    public void PauseGame()
    {
        if (!isGameActive || isGameFinished) return;

        isGamePaused = true;
        isGameActive = false;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        Debug.Log("Game paused");
    }

    public void ResumeGame()
    {
        if (isGameFinished) return;
        if (!isGamePaused) return;

        isGamePaused = false;
        isGameActive = true;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(CountdownTimer());

        Debug.Log("Game resumed");
    }

    public void ReplayGame()
    {
        Debug.Log("ReplayGame called - Resetting all values");

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        isGameActive = false;
        isGameFinished = false;
        isGamePaused = false;

        currentSteps = 0;
        currentTime = totalTime;

        UpdateStepDisplay();
        UpdateTimeDisplay();

        if (winPanel != null)
            winPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        Debug.Log($"Game replay ready: Time={currentTime}s, Steps={currentSteps}/{totalSteps}");
    }

    private IEnumerator CountdownTimer()
    {
        while (currentTime > 0 && !isGameFinished && !isGamePaused)
        {
            yield return new WaitForSeconds(1f);

            if (!isGameFinished && isGameActive && !isGamePaused)
            {
                currentTime--;
                UpdateTimeDisplay();
                Debug.Log($"Time left: {currentTime}s");
            }
        }

        if (!isGameFinished && isGameActive && !isGamePaused && currentTime <= 0)
        {
            Defeat();
        }
    }

    public void AddStep()
    {
        if (!isGameActive || isGameFinished || isGamePaused) return;

        currentSteps++;
        UpdateStepDisplay();

        Debug.Log($"Step added: {currentSteps}/{totalSteps}");

        OnStepUpdated?.Invoke(currentSteps);

        if (currentSteps >= totalSteps)
        {
            Win();
        }
    }

    private void Win()
    {
        if (isGameFinished) return;

        isGameActive = false;
        isGameFinished = true;
        isGamePaused = false;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Debug.Log("You win!");
        }

        OnGameWin?.Invoke();
    }

    private void Defeat()
    {
        if (isGameFinished) return;

        isGameActive = false;
        isGameFinished = true;
        isGamePaused = false;

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
            Debug.Log("You lose! Time's up!");
        }

        OnGameDefeat?.Invoke();
    }

    private void UpdateStepDisplay()
    {
        if (stepText != null)
        {
            stepText.text = $"{currentSteps}/{totalSteps}";
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            timeText.text = FormatTime(currentTime);
        }
    }

    private string FormatTime(int seconds)
    {
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }

    public void ResetGame()
    {
        LoadDataFromUI();
        isGameActive = false;
        isGameFinished = false;
        isGamePaused = false;

        if (winPanel != null)
            winPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        Debug.Log($"Game reset: Time={currentTime}s, Steps={currentSteps}/{totalSteps}");
    }

    public bool IsGameActive()
    {
        return isGameActive && !isGameFinished && !isGamePaused;
    }

    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public int GetCurrentSteps()
    {
        return currentSteps;
    }

    public int GetTotalSteps()
    {
        return totalSteps;
    }
}