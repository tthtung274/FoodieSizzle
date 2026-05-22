using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour  // Đã đổi tên class
{
    [Header("UI References")]
    public TMP_Text timeText;
    public TMP_Text stepText;

    [Header("Popup Panels")]
    public GameObject winPanel;
    public GameObject defeatPanel;

    // Internal variables
    private int currentSteps = 0;
    private int totalSteps = 0;
    private int currentTime = 0;
    private int totalTime = 0;
    private bool isGameActive = false;
    private bool isGameFinished = false;
    private Coroutine timerCoroutine;
    private DragDropManager dragDropManager;

    // Events
    public System.Action<int> OnStepUpdated;
    public System.Action OnGameWin;
    public System.Action OnGameDefeat;

    // Singleton
    public static ScoreManager Instance { get; private set; }  // Đã đổi tên

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
        // Hide panels
        if (winPanel != null)
            winPanel.SetActive(false);
        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        // Đọc dữ liệu từ UI text có sẵn
        LoadDataFromUI();
    }

    // Đọc dữ liệu từ UI Text
    private void LoadDataFromUI()
    {
        // Đọc steps từ stepText (ví dụ: "0/15")
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

        // Đọc thời gian từ timeText (ví dụ: "05:00")
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

        // Reset game state nhưng chưa active
        isGameActive = false;
        isGameFinished = false;
    }

    // Cập nhật lại dữ liệu từ UI (gọi khi load level mới)
    public void RefreshFromUI()
    {
        LoadDataFromUI();

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        Debug.Log($"Refreshed: Steps={currentSteps}/{totalSteps}, Time={currentTime}s");
    }

    private void Update()
    {
        // Chỉ bắt đầu game khi kéo thả lần đầu và game chưa active
        if (!isGameActive && !isGameFinished && dragDropManager != null)
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

        // Đảm bảo đọc lại dữ liệu mới nhất từ UI
        LoadDataFromUI();

        isGameActive = true;

        // Bắt đầu đếm ngược
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(CountdownTimer());

        Debug.Log($"Game started! Time: {currentTime}s, Steps: {currentSteps}/{totalSteps}");
    }

    private IEnumerator CountdownTimer()
    {
        while (currentTime > 0 && !isGameFinished)
        {
            yield return new WaitForSeconds(1f);

            if (!isGameFinished && isGameActive)
            {
                currentTime--;
                UpdateTimeDisplay();
                Debug.Log($"Time left: {currentTime}s");
            }
        }

        // Hết giờ
        if (!isGameFinished && isGameActive)
        {
            Defeat();
        }
    }

    public void AddStep()
    {
        if (!isGameActive || isGameFinished) return;

        currentSteps++;
        UpdateStepDisplay();

        Debug.Log($"Step added: {currentSteps}/{totalSteps}");

        OnStepUpdated?.Invoke(currentSteps);

        // Kiểm tra win
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
        LoadDataFromUI(); // Đọc lại từ UI
        isGameActive = false;
        isGameFinished = false;

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
        return isGameActive && !isGameFinished;
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