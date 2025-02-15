using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameMode gameMode;

    public TextMeshProUGUI totalScoreText;
    public TextMeshProUGUI averageScoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameWinPanel;
    public GameObject gameOverPanel;
    public TMP_Dropdown gameModeDropDown;
    public TMP_Dropdown gameLevelDropDown;

    public event Action<int> OnLevelTypeChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        OnLevelTypeChanged = null;
    }

    private void OnEnable()
    {
        gameModeDropDown.options.Clear();
        gameLevelDropDown.options.Clear();

        foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
        {
            gameModeDropDown.options.Add(new TMP_Dropdown.OptionData(mode.ToString()));
        }
        foreach (string s in Enum.GetNames(typeof(LevelType)))
        {
            gameLevelDropDown.options.Add(new TMP_Dropdown.OptionData(s));
        }

        // Add listener for when the value of the Dropdown changes
        gameModeDropDown.onValueChanged.AddListener(delegate { OnGameModeDropDownValueChanged(gameModeDropDown); });
        gameLevelDropDown.onValueChanged.AddListener(delegate { OnGameLevelValueChanged(gameLevelDropDown.value); });

        // Set default value
        gameModeDropDown.value = (int)gameMode;

    }
    void Update()
    {
        timerText.text = GameManager.Instance.timeRemaining.ToString("F2");
    }
    private void OnGameLevelValueChanged(int value)
    {
        OnLevelTypeChanged?.Invoke(value);
    }
    private void OnDisable()
    {
        gameModeDropDown.onValueChanged.RemoveAllListeners();
    }

    private void OnGameModeDropDownValueChanged(TMP_Dropdown change)
    {
        GameMode selectedMode = (GameMode)change.value;
        SetMode(selectedMode);
    }

    public void SetMode(GameMode mode)
    {
        gameMode = mode;
        GameManager.Instance.StartGame(gameMode);
    }
    public void UpdateUI()
    {
        totalScoreText.text = "Total Score: " + GameManager.Instance.totalScore;
        averageScoreText.text = "Average Score: " + GameManager.Instance.averageScorePerWord.ToString("F2");
    }
    public void ShowGameWinPanel()
    {
        gameWinPanel.SetActive(true);
    }
    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
    }
    public void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
