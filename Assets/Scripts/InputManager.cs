using UnityEngine;

public class InputManager : MonoBehaviour
{
    private Tile selectedTile;
    private bool isDragging = false;
    private Vector2 mousePos;
    private Camera camera;
    private void Start()
    {
        camera = Camera.main;
    }
    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null && !tile.IsBlocked())
                {
                    selectedTile = tile;
                    GridManager.Instance.StartDrag(selectedTile);
                    isDragging = true;
                }
            }
        }

        if (isDragging)
        {
            mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null && tile != selectedTile && !tile.IsBlocked())
                {
                    GridManager.Instance.AddTileToSelection(tile);
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            GridManager.Instance.EndDrag();
        }
    }
}
