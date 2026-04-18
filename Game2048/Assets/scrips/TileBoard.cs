using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileBoard : MonoBehaviour
{
    private Vector2 startTouch;
    private Vector2 swipeDelta;
    private bool isSwiping;
    
    public GameManager gameManager;
    public Tile tilePrefab;
    public TileState[] tileStates;
    
    private TileGrid grid;
    private List<Tile> tiles;
    private bool isWaiting; 
    private List<int> flashedNumbers = new List<int>();

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (var tile in tiles)
        {
            if (tile != null) Destroy(tile.gameObject);
        }
        
        tiles.Clear();
        flashedNumbers.Clear();
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform, false);
        tile.SetState(tileStates[0], 2);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    private void Update()
    {
        HandleTouchInput();

        if (!isWaiting)
        {
            
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveTiles(Vector2Int.down, 0, 1, 1, 1);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveTiles(Vector2Int.up, 0, 1, grid.height - 2, -1);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
            }
        }
    }

    private void HandleTouchInput()
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen == null) return;

        var touch = touchscreen.primaryTouch;

       
        if (touch.press.wasPressedThisFrame)
        {
            startTouch = touch.position.ReadValue();
            isSwiping = true;
        }

        
        if (isSwiping && touch.press.isPressed)
        {
            swipeDelta = touch.position.ReadValue() - startTouch;

            // 
            if (swipeDelta.magnitude > 80f)
            {
                ExecuteSwipe();
                isSwiping = false; 
            }
        }

        // ৩. mouse tach sere dile reset
        if (touch.press.wasReleasedThisFrame)
        {
            isSwiping = false;
        }
    }

    private void ExecuteSwipe()
    {
        if (isWaiting) return;

        float x = swipeDelta.x;
        float y = swipeDelta.y;

        if (Mathf.Abs(x) > Mathf.Abs(y)) // Horizontal swipe
        {
            if (x > 0)
                MoveTiles(Vector2Int.right, grid.width - 2, -1, 0, 1);
            else
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
        }
        else // Vertical swipe
        {
            if (y > 0)
                MoveTiles(Vector2Int.down, 0, 1, 1, 1);
            else
                MoveTiles(Vector2Int.up, 0, 1, grid.height - 2, -1);
        }
    }

    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);
                if (cell.occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }
        
        if (changed)
        {
            StartCoroutine(WaitForChange());
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                if (CanMerge(tile, adjacent.tile)) 
                {
                    Merge(tile, adjacent.tile);
                    return true;
                }
                break;
            }
            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell); 
            return true;
        }
        
        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);
        
        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;

        b.SetState(tileStates[index], number);

        if (!flashedNumbers.Contains(number))
        {
            b.Bounce();
            flashedNumbers.Add(number);
        }
        
        gameManager.IncreaseScore(number);
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i]) return i;
        }
        return -1;
    }

    private IEnumerator WaitForChange()
    {
        isWaiting = true;
        yield return new WaitForSeconds(0.1f);
        isWaiting = false;

        foreach (var tile in tiles)
        {
            tile.locked = false;
        }

        if (tiles.Count != grid.size)
        {
            CreateTile();
        }

        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
    }

    private bool CheckForGameOver()
    {
        if (tiles.Count < grid.size) return false;

        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if (up != null && CanMerge(tile, up.tile)) return false;
            if (down != null && CanMerge(tile, down.tile)) return false;
            if (left != null && CanMerge(tile, left.tile)) return false;
            if (right != null && CanMerge(tile, right.tile)) return false;
        }

        return true;
    }
}