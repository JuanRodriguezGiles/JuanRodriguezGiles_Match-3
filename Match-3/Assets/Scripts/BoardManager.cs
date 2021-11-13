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
    }

    void OnDisable()
    {
        PlayerInput.OnMouseReleased -= ClearBlocks;
        UiGameplay.OnPlayButtonPressed -= RestartGrid;
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
        float x = (float)rows / 2 - 0.5f;
        float y = (float)columns / 2;
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
                    BlockObjectPool.Get().Pool.Release(_grid[i, j].prefab);
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
        yield return new WaitForSeconds(1);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (!_grid[i, j].active)
                {
                    _grid[i, j].prefab = BlockObjectPool.Get().Pool.Get();
                    _grid[i, j].prefab.transform.position = new Vector3(_grid[i, j].column, _grid[i, j].row, 0);
                    _grid[i, j].prefab.transform.rotation = Quaternion.identity;
                    _grid[i, j].prefab.name = "Block";
                    _grid[i, j].SetBlockType(Random.Range(0, blockTypes.Count));
                    _grid[i, j].active = true;
                }
            }
        }
        yield return new WaitForSeconds(0.2f);
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
        StartCoroutine(RefillGrid());
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
            if (!GameManager.Get().CheckForGameOver())
            {
                StartCoroutine(RefillGrid());
            }
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
        {
            ////Up
            //for (int i = 0; i < minMatchNumber; i++)
            //{
            //    if (pos.y + minMatchNumber - 1 >= rows)
            //        break;
            //    matchedBlocks.Add(_grid[(int)pos.y + i, (int)pos.x].prefab);
            //}
            //if (matchedBlocks.All(_blocks => _blocks.CompareTag(block.tag)) && matchedBlocks.Count >= minMatchNumber &&
            //    !matched)
            //{
            //    ClearCombo(matchedBlocks);
            //    matched = true;
            //}
            ////Down
            //for (int i = 0; i < minMatchNumber; i++)
            //{
            //    if (pos.y - minMatchNumber - 1 <= 0)
            //        break;
            //    matchedBlocks.Add(_grid[(int)pos.y - i, (int)pos.x].prefab);
            //}
            //if (matchedBlocks.All(_blocks => _blocks.CompareTag(block.tag)) && matchedBlocks.Count >= minMatchNumber &&
            //    !matched)
            //{
            //    matched = true;
            //    ClearCombo(matchedBlocks);
            //}
            ////Left
            //for (int i = 0; i < minMatchNumber; i++)
            //{
            //    if (pos.x - minMatchNumber - 1 <= 0)
            //        break;
            //    matchedBlocks.Add(_grid[(int)pos.y, (int)pos.x - i].prefab);
            //}
            //if (matchedBlocks.All(_blocks => _blocks.CompareTag(block.tag)) && matchedBlocks.Count >= minMatchNumber &&
            //    !matched)
            //{
            //    matched = true;
            //    ClearCombo(matchedBlocks);
            //}
            ////Right
            //for (int i = 0; i < minMatchNumber; i++)
            //{
            //    if (pos.x + minMatchNumber - 1 >= columns)
            //        break;
            //    matchedBlocks.Add(_grid[(int)pos.y, (int)pos.x + i].prefab);
            //}
            //if (matchedBlocks.All(_blocks => _blocks.CompareTag(block.tag)) && matchedBlocks.Count >= minMatchNumber &&
            //    !matched)
            //{
            //    matched = true;
            //    ClearCombo(matchedBlocks);
            //}
        }
        //PlayerInput.allowed = true;
    }
    #endregion
}