using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public class BoardManager : MonoBehaviour
{
    #region PROPERTIES
    private int rows;
    private int columns;
    private SPAWN_TYPES spawnType;
    private List<BLOCK_TYPES> blockTypes;
    private float spawnTime;
    private int minMatchNumber;
    private bool moving = false;
    public Block[,] _grid;

    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;

    public static event Action OnClearBlocks;
    public static event Action OnMatch;
    public static event Action OnNoMatch;
    #endregion

    #region METHODS
    void OnEnable()
    {
        PlayerInput.OnMouseReleased += ClearBlocks;
        UiGameplay.OnPlayButtonPressed += RestartGrid;
        ReleaseBlock.OnDespawnAnimationDone += MoveGrid;
    }

    void OnDisable()
    {
        PlayerInput.OnMouseReleased -= ClearBlocks;
        UiGameplay.OnPlayButtonPressed -= RestartGrid;
        ReleaseBlock.OnDespawnAnimationDone -= MoveGrid;
    }

    void Start()
    {
        rows = GameManager.Get().rows;
        columns = GameManager.Get().columns;
        spawnType = GameManager.Get().spawnType;
        blockTypes = GameManager.Get().blockTypes;
        spawnTime = GameManager.Get().spawnTime;
        minMatchNumber = GameManager.Get().minMatchNumber;

        CreateGrid();
        CenterCameraOnGrid();
        StartCoroutine(InstantiateBlocks(false));
    }

    void CreateGrid()
    {
        _grid = new Block[rows, columns];
        GameObject grid = new GameObject("Grid");
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(j, i);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, grid.transform);
                tile.name = "Tile " + i + " " + j;
            }
        }
    }

    void RestartGrid()
    {
        StartCoroutine(InstantiateBlocks(true));
    }

    void CenterCameraOnGrid()
    {
        Camera camera = FindObjectOfType<Camera>();
        int x = columns / 2;
        int y = rows / 2;
        Vector3 position = new Vector3(x, y, -100);
        camera.transform.position = position;
    }

    IEnumerator InstantiateBlocks(bool restart)
    {
        BLOCK_TYPES[] previousLeft = new BLOCK_TYPES[rows];
        BLOCK_TYPES previousDown = BLOCK_TYPES.AIR;

        if (restart)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (_grid[i, j].active)
                    {
                        BlockObjectPool.Get().Pool.Release(_grid[i, j].prefab);
                    }
                }
            }
        }

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = spawnType switch
                {
                    SPAWN_TYPES.LEFT_RIGHT_UP => new Vector2(j, i),
                    SPAWN_TYPES.RIGHT_LEFT_UP => new Vector2(columns - 1 - j, i),
                    SPAWN_TYPES.LEFT_RIGHT_DOWN => new Vector2(j, rows - 1 - i),
                    SPAWN_TYPES.RIGHT_LEFT_DOWN => new Vector2(columns - 1 - j, rows - 1 - i),
                    _ => throw new ArgumentOutOfRangeException()
                };

                Block block = new Block();
                block.prefab = BlockObjectPool.Get().Pool.Get();
                block.prefab.transform.position = position;
                block.prefab.transform.rotation = Quaternion.identity;
                block.prefab.name = "Block";
                block.active = true;

                _grid[(int)position.y, (int)position.x] = block;
                _grid[(int)position.y, (int)position.x].column = (int)position.x;
                _grid[(int)position.y, (int)position.x].row = (int)position.y;

                List<BLOCK_TYPES> possibleBlocks = new List<BLOCK_TYPES>();
                possibleBlocks.AddRange(blockTypes);

                possibleBlocks.Remove(previousLeft[j]);
                possibleBlocks.Remove(previousDown);

                int type = (int)possibleBlocks[Random.Range(0, possibleBlocks.Count)];
                block.SetBlockType(type);

                previousLeft[j] = (BLOCK_TYPES)type;
                previousDown = (BLOCK_TYPES)type;

                yield return new WaitForSeconds(spawnTime);
            }
        }

        PlayerInput.allowed = true;
    }

    IEnumerator RefillGrid()
    {
        PlayerInput.allowed = false;
        while (moving)
        {
            yield return new WaitForSeconds(1);
        }
        for (int j = 0; j < columns; j++)
        {
            int emptyCount = 0;
            for (int i = 0; i < rows; i++)
            {
                if (_grid[i, j].active) continue;
                emptyCount++;
                _grid[i, j].prefab = BlockObjectPool.Get().Pool.Get();
                _grid[i, j].prefab.transform.position = new Vector3(_grid[i, j].column, rows + emptyCount, 0);
                _grid[i, j].prefab.transform.rotation = Quaternion.identity;
                _grid[i, j].prefab.name = "Block";
                _grid[i, j].SetBlockType(Random.Range(0, blockTypes.Count));
                _grid[i, j].active = true;
                StartCoroutine(DropBlock(_grid[i, j].prefab, rows + emptyCount, i));
            }
        }
        while (moving)
        {
            yield return new WaitForSeconds(1);
        }
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (_grid[i, j].active)
                {
                    CheckMatches(_grid[i, j].prefab);
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
        PlayerInput.allowed = true;
    }

    void ClearCombo(List<GameObject> matchedBlocks)
    {
        OnMatch?.Invoke();
        PlayerInput.allowed = false;
        foreach (var block in matchedBlocks)
        {
            if (!block) continue;
            _grid[(int)block.transform.position.y, (int)block.transform.position.x].active = false;
            block.GetComponent<Animator>().SetTrigger("OnDespawn");
        }
        GameManager.Get().AddScore(matchedBlocks.Count);
    }

    List<Vector2> GetEmptyTiles()
    {
        List<Vector2> tiles = new List<Vector2>();
        for (int i = 0; i < rows; i++) //Find empty tiles
        {
            for (int j = 0; j < columns; j++)
            {
                Collider2D block = Physics2D.OverlapPoint(new Vector2(j, i));
                if (!block)
                {
                    tiles.Add(new Vector2(j, i));
                }
            }
        }
        return tiles;
    }

    void MoveGrid()
    {
        MoveGridDown();
        StartCoroutine(RefillGrid());
    }

    void ClearBlocks(List<GameObject> selectedBlocks)
    {
        if (selectedBlocks.Count >= minMatchNumber)
        {
            PlayerInput.allowed = false;
            OnMatch?.Invoke();
            foreach (var block in selectedBlocks)
            {
                if (!block) continue;
                _grid[(int)block.transform.position.y, (int)block.transform.position.x].active = false;
                block.GetComponent<Animator>().SetTrigger("OnDespawn");
            }
            GameManager.Get().AddScore(selectedBlocks.Count);
            GameManager.Get().UpdateMovesLeft();
            GameManager.Get().CheckForGameOver();
        }
        else
        {
            OnNoMatch?.Invoke();
            foreach (var block in selectedBlocks)
            {
                block.GetComponent<SpriteRenderer>().color = Color.white;
                block.GetComponent<Animator>().SetBool("Selected", false);
            }
        }
        selectedBlocks.Clear();
        OnClearBlocks?.Invoke();
    }

    void CheckMatches(GameObject block)
    {
        List<GameObject> matchedBlocks = new List<GameObject>();
        bool matched = false;
        Vector2 pos = new Vector2(block.transform.position.x, block.transform.position.y);
        {
            RaycastHit2D[] hitUp = Physics2D.RaycastAll(pos, Vector2.up, minMatchNumber - 1);
            RaycastHit2D[] hitDown = Physics2D.RaycastAll(pos, Vector2.down, minMatchNumber - 1);
            RaycastHit2D[] hitLeft = Physics2D.RaycastAll(pos, Vector2.left, minMatchNumber - 1);
            RaycastHit2D[] hitRight = Physics2D.RaycastAll(pos, Vector2.right, minMatchNumber - 1);

            if (hitUp.All(blocks => blocks.transform.gameObject.CompareTag(block.tag) && hitUp.Length >= minMatchNumber))
            {
                for (int i = 0; i < hitUp.Length; i++)
                {
                    matchedBlocks.Add(hitUp[i].transform.gameObject);
                }
                ClearCombo(matchedBlocks);
                matched = true;
            }
            if (hitDown.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag) && hitDown.Length >= minMatchNumber && !matched))
            {
                for (int i = 0; i < hitDown.Length; i++)
                {
                    matchedBlocks.Add(hitDown[i].transform.gameObject);
                }
                ClearCombo(matchedBlocks);
                matched = true;
            }
            if (hitLeft.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag) && hitLeft.Length >= minMatchNumber && !matched))
            {
                for (int i = 0; i < hitLeft.Length; i++)
                {
                    matchedBlocks.Add(hitLeft[i].transform.gameObject);
                }
                ClearCombo(matchedBlocks);
                matched = true;
            }
            if (hitRight.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag) && hitRight.Length >= minMatchNumber && !matched))
            {
                for (int i = 0; i < hitRight.Length; i++)
                {
                    matchedBlocks.Add(hitRight[i].transform.gameObject);
                }
                ClearCombo(matchedBlocks);
                matched = true;
            }
        }
    }

    void MoveGridDown()
    {
        int tileCounter = 1;

        for (int j = 0; j < columns; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                if (i + tileCounter < rows && !_grid[i, j].active)
                {
                    while (i + tileCounter < rows - 1 && !_grid[i + tileCounter, j].active)
                    {
                        tileCounter++;
                    }

                    Block aux = _grid[i, j];
                    _grid[i, j] = _grid[i + tileCounter, j];
                    _grid[i, j].row = i;

                    _grid[i + tileCounter, j] = aux;
                    _grid[i + tileCounter, j].row = i + tileCounter;
                    _grid[i + tileCounter, j].active = false;

                    StartCoroutine(DropBlock(_grid[i, j].prefab, i + tileCounter, i));
                }
                tileCounter = 1;
            }
        }
    }

    IEnumerator DropBlock(GameObject block, float startY, float targetY)
    {
        moving = true;
        float speed = 4f;
        block.transform.position = new Vector3(block.transform.position.x, startY, 0);
        while (block.transform.position.y > targetY)
        {
            block.transform.position -= speed * Time.deltaTime * new Vector3(0, 1, 0);
            yield return null;
        }
        block.transform.position = new Vector3(block.transform.position.x, targetY, block.transform.position.z);
        moving = false;
    }

    #endregion
}