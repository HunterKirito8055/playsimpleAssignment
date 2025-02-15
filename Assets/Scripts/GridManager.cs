using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private Tile letterTilePrefab;

    private Tile[,] grid;
    private List<Tile> selectedTiles = new List<Tile>();
    private HashSet<string> formedWords = new HashSet<string>();
    private string selectedWord;

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
    }

    public void InitializeGrid()
    {
        var config = GameManager.Instance.LevelConfig;
        if (grid == null)
        {
            grid = new Tile[config.gridSize.x, config.gridSize.y];
        }
        for (int x = 0, index = 0; x < config.gridSize.x; x++)
        {
            for (int y = 0; y < config.gridSize.y; y++, index++)
            {
                Tile tile = GetTileAtPosition(x, y);
                if (tile != null)
                {
                    tile.transform.position = new Vector2(x, y);
                }
                else
                {
                    tile = Instantiate(letterTilePrefab, new Vector2(x, y), Quaternion.identity);
                }
                tile.Initialize(config.gridData[index], x, y);
                grid[x, y] = tile;
            }
        }
        ResetWordsFormed();
        CenterCamera(config.gridSize);
    }

    // Reset the words formed
    private void ResetWordsFormed()
    {
        formedWords.Clear();
    }

    // Calculate the grid's center position
    private void CenterCamera(Vector2Int gridSize)
    {
        if (gridSize == Vector2Int.zero)
        {
            return;
        }
        Camera mainCamera = Camera.main;
        mainCamera.transform.position = new Vector3((gridSize.x - 1) / 2f, (gridSize.y - 1) / 2f, -10);

        float aspectRatio = (float)Screen.width / Screen.height;
        mainCamera.orthographicSize = Mathf.Max(gridSize.x / (2f * aspectRatio), gridSize.y / 2f);
    }

    // Called when the player starts dragging
    public void StartDrag(Tile tile)
    {
        selectedTiles.Clear();
        selectedWord = string.Empty;
        AddTileToSelection(tile);
    }

    // Called when the player drags over a tile
    public void AddTileToSelection(Tile tile)
    {
        if (!selectedTiles.Contains(tile) && IsAdjacent(tile) && !tile.IsBlocked())
        {
            selectedTiles.Add(tile);
            selectedWord += tile.letter;
            tile.Select();
        }
    }

    // Check if the tile is adjacent to the last selected tile
    private bool IsAdjacent(Tile tile)
    {
        if (selectedTiles.Count == 0) return true; // First tile is always valid

        Tile lastSelectedTile = selectedTiles[^1];
        int dx = Mathf.Abs(tile.gridX - lastSelectedTile.gridX);
        int dy = Mathf.Abs(tile.gridY - lastSelectedTile.gridY);
        return dx <= 1 && dy <= 1; // Adjacent tiles are within 1 unit distance
    }

    // Called when the player releases the mouse button
    public void EndDrag()
    {
        if (selectedTiles.Count > 1)
        {
            CheckWord();
        }
        ClearSelectedTiles();
    }

    // Validate the selected word
    private void CheckWord()
    {
        if (formedWords.Contains(selectedWord))
        {
            Debug.Log("Word already formed");
            return;
        }

        if (WordValidator.Instance.IsValidWord(selectedWord))
        {
            int wordScore = CalculateWordScore();

            selectedTiles.ForEach((_tile) => TryUnblockAdjecentTiles(_tile));

            formedWords.Add(selectedWord);
            GameManager.Instance.OnWordFormed(wordScore);
            Debug.Log("Valid Word: " + selectedWord + " Score: " + wordScore);
        }
        else
        {
            Debug.Log("Invalid Word");
        }
    }
    private void TryUnblockAdjecentTiles(Tile tile)
    {
        Vector2Int tilePosition = new Vector2Int(tile.gridX, tile.gridY);
        Tile temp = null;
        if (tilePosition.x >= 0 && tilePosition.y >= 0)
        {
            // Check left
            if (tilePosition.x > 0)
            {
                temp = (GetTileAtPosition(tilePosition.x - 1, tilePosition.y));
                temp?.TryUnblock();
            }
            // Check right
            if (tilePosition.x < grid.GetLength(0) - 1)
            {
                temp = (GetTileAtPosition(tilePosition.x + 1, tilePosition.y));
                temp?.TryUnblock();
            }
            // Check up
            if (tilePosition.y < grid.GetLength(1) - 1)
            {
                temp = (GetTileAtPosition(tilePosition.x, tilePosition.y + 1));
                temp?.TryUnblock();
            }
            // Check down
            if (tilePosition.y > 0)
            {
                temp = (GetTileAtPosition(tilePosition.x, tilePosition.y - 1));
                temp?.TryUnblock();
            }
        }
    }

    // Calculate the score for the word
    private int CalculateWordScore()
    {
        int score = 0;
        foreach (Tile tile in selectedTiles)
        {
            if (tile.tileType == Tile.TileType.BONUS)
            {
                GameManager.Instance.OnBonusLetterFoundAction?.Invoke(tile.letter);
            }
            score += tile.GetScore();
        }
        return score;
    }

    private Tile GetTileAtPosition(int x, int y)
    {
        if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
        {
            return null;
        }
        return grid[x, y];
    }

    // Reset the selected tiles
    private void ClearSelectedTiles()
    {
        foreach (Tile tile in selectedTiles)
        {
            tile.Deselect();
        }
        selectedTiles.Clear();
    }
}

[System.Serializable]
public struct GridData
{
    public int tileType;
    public string letter;
}

[System.Serializable]
public struct LevelConfig
{
    public int bugCount;
    public int wordCount;
    public int timeSec;
    public int totalScore;
    public Vector2Int gridSize;
    public List<GridData> gridData;
}

public enum LevelType
{
    WORD_COUNT_GOAL, // Make X words
    SCORE_WITHIN_TIME, // Reach X score in Y time
    WORDS_WITHIN_TIME, // Make X words in Y time
    BONUS_WORD_COUNT_GOAL // Collect X bonus words
}
