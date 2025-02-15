using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode { Levels, Endless }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const string LEVEL_DATA_SCORE_IN_TIME = "LevelData_Score_in_Time";
    public const string LEVEL_DATA_WORDS_IN_TIME = "LevelData_Words_in_TIme";
    public const string LEVEL_DATA_BUG_COUNT = "LevelData_BugCount";
    public const string LEVEL_DATA_WORD_COUNT = "LevelData_WordCount";

    public HashSet<char> bonusLetters = new HashSet<char>();
    public enum GameOverType { NONE, WIN, LOSE }

    public GameMode currentMode;
    public LevelType currentLevelType;

    public LevelConfig LevelConfig { private set; get; }
    public Action<char> OnBonusLetterFoundAction;

    public int totalScore = 0;
    public int wordsFormed = 0;
    public float averageScorePerWord = 0;
    public float timeRemaining = 0;
    private GameOverType gameOverType = GameOverType.NONE;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        OnBonusLetterFoundAction = null;
    }

    private void Start()
    {
        OnBonusLetterFoundAction += OnBonusLetterFound;
        UIManager.Instance.OnLevelTypeChanged += OnLevelTypeChangedHandler;
        InitializeLevelData();
    }
    private void OnDestroy()
    {
        UIManager.Instance.OnLevelTypeChanged -= OnLevelTypeChangedHandler;
    }
    private void OnLevelTypeChangedHandler(int _obj)
    {
        currentLevelType = (LevelType)_obj;
        InitializeLevelData();
    }
    private void Update()
    {
        if (gameOverType == (GameOverType.WIN | GameOverType.LOSE))
        {
            return;
        }

        if (LevelConfig.timeSec > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0)
            {
                CheckCompletion();
            }
        }
    }

    private void InitializeLevelData()
    {
        string levelData = "";
        switch (currentLevelType)
        {
            case LevelType.WORD_COUNT_GOAL:
                levelData = LEVEL_DATA_WORD_COUNT;
                break;
            case LevelType.SCORE_WITHIN_TIME:
                levelData = LEVEL_DATA_SCORE_IN_TIME;
                break;
            case LevelType.WORDS_WITHIN_TIME:
                levelData = LEVEL_DATA_WORDS_IN_TIME;
                break;
            case LevelType.BONUS_WORD_COUNT_GOAL:
                levelData = LEVEL_DATA_BUG_COUNT;
                break;
        }

        TextAsset jsonFile = Resources.Load<TextAsset>(levelData);
        if (jsonFile != null)
        {
            string json = jsonFile.text;
            LevelConfig config = JsonUtility.FromJson<LevelConfig>(json);
            LevelConfig = config;
        }
        StartGame(currentMode);
      
    }

    public void StartGame(GameMode mode)
    {
        if (mode == GameMode.Endless)
        {
            return;
        }

        currentMode = mode;
        totalScore = 0;
        wordsFormed = 0;
        averageScorePerWord = 0;
        timeRemaining = LevelConfig.timeSec;

        GridManager.Instance.InitializeGrid();
        UIManager.Instance.UpdateUI();
    }

    public void OnWordFormed(int wordScore)
    {
        totalScore += wordScore;
        wordsFormed++;
        averageScorePerWord = (float)totalScore / wordsFormed;

        CheckCompletion();
        UIManager.Instance.UpdateUI();
    }
    private void OnBonusLetterFound(char c)
    {
        bonusLetters.Add(c);
    }

    private void CheckCompletion()
    {
        bool isTimesUp = (timeRemaining <= 0);
        bool isScoreMet = false;

        switch (currentLevelType)
        {
            // Time is not considered in this level
            case LevelType.WORD_COUNT_GOAL:
                if (wordsFormed >= LevelConfig.wordCount)
                {
                    isScoreMet = true;
                }
                isTimesUp = false;
                break;
            // Time is not considered in this level
            case LevelType.BONUS_WORD_COUNT_GOAL:
                if (bonusLetters.Count >= LevelConfig.bugCount)
                {
                    isScoreMet = true;
                }
                isTimesUp = false;
                break;
            case LevelType.SCORE_WITHIN_TIME:
                if (totalScore >= LevelConfig.totalScore)
                {
                    isScoreMet = true;
                }
                break;
            case LevelType.WORDS_WITHIN_TIME:
                if (wordsFormed >= LevelConfig.wordCount && timeRemaining > 0)
                {
                    isScoreMet = true;
                }
                break;
        }
        if (isScoreMet)
        {
            CompleteLevel();
        }
        else if (isTimesUp)
        {
            FailLevel();
        }
    }

    private void CompleteLevel()
    {
        gameOverType = GameOverType.WIN;
        UIManager.Instance.ShowGameWinPanel();
    }

    private void FailLevel()
    {
        gameOverType = GameOverType.LOSE;
        UIManager.Instance.ShowGameOverPanel();
    }
}
