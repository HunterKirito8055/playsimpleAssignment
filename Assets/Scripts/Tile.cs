using TMPro;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType
    {
        NORMAL,
        BLOCKED,
        BONUS
    }

    public TileType tileType;
    public char letter;
    public int gridX, gridY;
    public TextMeshPro letterText;
    public Sprite normalSprite;
    public Sprite blockedSprite;
    public Sprite bonusSprite;

    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void UpdateTileSprite()
    {
        switch (tileType)
        {
            case TileType.NORMAL:
                spriteRenderer.sprite = normalSprite;
                break;
            case TileType.BLOCKED:
                spriteRenderer.sprite = blockedSprite;
                break;
            case TileType.BONUS:
                spriteRenderer.sprite = bonusSprite;
                break;
        }
    }
    public void Initialize(GridData gridData, int x, int y)
    {
        this.letter = gridData.letter[0];
        this.gridX = x;
        this.gridY = y;
        letterText.text = letter.ToString();
        SetTileType((TileType)gridData.tileType);
    }
    public void SetTileType(TileType type)
    {
        tileType = type;
        UpdateTileSprite();
    }
    public void Initialize(char letter, int x, int y)
    {
        this.letter = letter;
        this.gridX = x;
        this.gridY = y;
        letterText.text = letter.ToString();
    }

    public int GetScore()
    {
        int score = GetLetterScore(this.letter);
        return tileType == TileType.BONUS ? score * 2 : score;
    }

    /// <summary>
    /// Simple scoring: A=1, B=2, ..., Z=26
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    private int GetLetterScore(char c)
    {
        return c - 'A' + 1;
    }

    public bool TryUnblock()
    {
        if (IsBlocked())
        {
            Unblock();
            return true;
        }
        return false;
    }
    public bool IsBlocked()
    {
        return tileType == TileType.BLOCKED;
    }

    public void Unblock()
    {
        if (tileType == TileType.BLOCKED)
        {
            SetTileType(TileType.NORMAL);
        }
    }

    // Visual feedback for selection
    public void Select()
    {
        GetComponent<SpriteRenderer>().color = Color.green; // Highlight the tile
    }

    // Reset the tile to its original state
    public void Deselect()
    {
        GetComponent<SpriteRenderer>().color = originalColor; // Reset the color
    }
}
