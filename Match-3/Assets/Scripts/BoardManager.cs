using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public class BoardManager : MonoBehaviour
{
    #region PROPERTIES
    private GameData gameData;
    private bool blocksMoving;
    private Block[,] grid;

    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;

    public static event Action OnClearBlocks;
    public static event Action OnMatch;
    public static event Action OnNoMatch;

    private GameObject blocksParent;
    #endregion

    #region METHODS
    void OnEnable()
    {
        PlayerInput.OnMouseReleased += TryMatch;
        UiGameplay.OnPlayButtonPressed += RestartGrid;
        ReleaseBlock.OnDespawnAnimationDone += UpdateGrid;
    }

    void OnDisable()
    {
        PlayerInput.OnMouseReleased -= TryMatch;
        UiGameplay.OnPlayButtonPressed -= RestartGrid;
        ReleaseBlock.OnDespawnAnimationDone -= UpdateGrid;
    }

    void Start()
    {
        gameData = GameManager.Get().gameData;
        blocksParent = new GameObject("Blocks");

        CreateGrid();
        CenterCameraOnGrid();
        StartCoroutine(SpawnBlocksOnGrid(false));
    }

    void CreateGrid()
    {
        this.grid = new Block[gameData.rows, gameData.columns];
        GameObject grid = new GameObject("Grid");
        for (int i = 0; i < gameData.rows; i++)
        {
            for (int j = 0; j < gameData.columns; j++)
            {
                Vector2 position = new Vector2(j, i);
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, grid.transform);
                tile.name = "Tile " + i + " " + j;
            }
        }
    }

    void CenterCameraOnGrid()
    {
        Camera camera = FindObjectOfType<Camera>();
        int x = gameData.columns / 2;
        int y = gameData.rows / 2;
        Vector3 position = new Vector3(x, y, -100);
        camera.transform.position = position;
    }

    IEnumerator SpawnBlocksOnGrid(bool restart)
    {
        BLOCK_TYPES[] previousLeft = new BLOCK_TYPES[gameData.rows];
        BLOCK_TYPES previousDown = BLOCK_TYPES.AIR;

        if (restart)
        {
            for (int i = 0; i < gameData.rows; i++)
            {
                for (int j = 0; j < gameData.columns; j++)
                {
                    if (grid[i, j].active)
                    {
                        BlockObjectPool.Get().Pool.Release(grid[i, j].prefab);
                    }
                }
            }
        }

        for (int i = 0; i < gameData.rows; i++)
        {
            for (int j = 0; j < gameData.columns; j++)
            {
                Vector2 position = gameData.spawnType switch
                {
                    SPAWN_TYPES.LEFT_RIGHT_UP => new Vector2(j, i),
                    SPAWN_TYPES.RIGHT_LEFT_UP => new Vector2(gameData.columns - 1 - j, i),
                    SPAWN_TYPES.LEFT_RIGHT_DOWN => new Vector2(j, gameData.rows - 1 - i),
                    SPAWN_TYPES.RIGHT_LEFT_DOWN => new Vector2(gameData.columns - 1 - j, gameData.rows - 1 - i),
                    _ => throw new ArgumentOutOfRangeException()
                };

                Block block = new Block();
                block.prefab = BlockObjectPool.Get().Pool.Get();
                block.prefab.transform.position = position;
                block.prefab.transform.rotation = Quaternion.identity;
                block.prefab.name = "Block";
                block.prefab.transform.parent = blocksParent.transform;
                block.active = true;

                grid[(int)position.y, (int)position.x] = block;
                grid[(int)position.y, (int)position.x].column = (int)position.x;
                grid[(int)position.y, (int)position.x].row = (int)position.y;

                List<BLOCK_TYPES> possibleBlocks = new List<BLOCK_TYPES>();
                possibleBlocks.AddRange(gameData.blockTypes);

                possibleBlocks.Remove(previousLeft[j]);
                possibleBlocks.Remove(previousDown);

                int type = (int)possibleBlocks[Random.Range(0, possibleBlocks.Count)];
                block.SetBlockType(type);

                previousLeft[j] = (BLOCK_TYPES)type;
                previousDown = (BLOCK_TYPES)type;

                yield return new WaitForSeconds(gameData.spawnTime);
            }
        }

        PlayerInput.allowed = true;
    }

    void RestartGrid()
    {
        StartCoroutine(SpawnBlocksOnGrid(true));
    }

    IEnumerator RefillGrid()
    {
        PlayerInput.allowed = false;

        while (blocksMoving)
        {
            yield return new WaitForSeconds(1);
        }

        for (int j = 0; j < gameData.columns; j++)
        {
            int emptyCount = 0;
            for (int i = 0; i < gameData.rows; i++)
            {
                if (grid[i, j].active) continue;
                emptyCount++;
                grid[i, j].prefab = BlockObjectPool.Get().Pool.Get();
                grid[i, j].prefab.transform.position = new Vector3(grid[i, j].column, gameData.rows + emptyCount, 0);
                grid[i, j].prefab.transform.rotation = Quaternion.identity;
                grid[i, j].prefab.name = "Block";
                grid[i, j].prefab.transform.parent = blocksParent.transform;
                grid[i, j].SetBlockType(Random.Range(0, gameData.blockTypes.Count));
                grid[i, j].active = true;
                StartCoroutine(UpdateBlockPosition(grid[i, j].prefab, gameData.rows + emptyCount, i));
            }
        }

        while (blocksMoving)
        {
            yield return new WaitForSeconds(1);
        }

        for (int i = 0; i < gameData.rows; i++)
        {
            for (int j = 0; j < gameData.columns; j++)
            {
                if (grid[i, j].active)
                {
                    CheckForCombos(grid[i, j].prefab);
                }
            }
        }

        PlayerInput.allowed = true;
        GameManager.Get().CheckForGameOver();
    }

    void TryMatch(List<GameObject> selectedBlocks)
    {
        if (selectedBlocks.Count >= gameData.minimumMatchNumber)
        {
            PlayerInput.allowed = false;
            OnMatch?.Invoke();
            foreach (var block in selectedBlocks)
            {
                if (!block) continue;
                grid[(int)block.transform.position.y, (int)block.transform.position.x].active = false;
                block.GetComponent<Animator>().SetTrigger("OnDespawn");
            }
            GameManager.Get().AddScore(selectedBlocks.Count);
            GameManager.Get().UpdateMovesLeft();
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

    void UpdateGrid()
    {
        ShiftBlocksDown();
        StopCoroutine(RefillGrid());
        StartCoroutine(RefillGrid());
    }

    void ShiftBlocksDown()
    {
        int tileCounter = 1;

        for (int j = 0; j < gameData.columns; j++)
        {
            for (int i = 0; i < gameData.rows; i++)
            {
                if (i + tileCounter < gameData.rows && !grid[i, j].active)
                {
                    while (i + tileCounter < gameData.rows - 1 && !grid[i + tileCounter, j].active)
                    {
                        tileCounter++;
                    }

                    Block aux = grid[i, j];
                    grid[i, j] = grid[i + tileCounter, j];
                    grid[i, j].row = i;

                    grid[i + tileCounter, j] = aux;
                    grid[i + tileCounter, j].row = i + tileCounter;
                    grid[i + tileCounter, j].active = false;

                    StartCoroutine(UpdateBlockPosition(grid[i, j].prefab, i + tileCounter, i));
                }
                tileCounter = 1;
            }
        }
    }

    IEnumerator UpdateBlockPosition(GameObject block, float startY, float targetY)
    {
        blocksMoving = true;
        float speed = 15;
        block.transform.position = new Vector3(block.transform.position.x, startY, 0);
        while (block.transform.position.y > targetY)
        {
            block.transform.position -= speed * Time.deltaTime * new Vector3(0, 1, 0);
            yield return new WaitForEndOfFrame();
        }
        block.transform.position = new Vector3(block.transform.position.x, targetY, block.transform.position.z);
        blocksMoving = false;
    }

    void CheckForCombos(GameObject block)
    {
        List<GameObject> matchedBlocks = new List<GameObject>();
        Vector2 position = new Vector2(block.transform.position.x, block.transform.position.y);

        bool matched = IsMatchPossible(position, Vector2.up, gameData.minimumMatchNumber - 1, block.tag);
        if (!matched)
        {
            matched = IsMatchPossible(position, Vector2.down, gameData.minimumMatchNumber - 1, block.tag);
        }
        if (!matched)
        {
            matched = IsMatchPossible(position, Vector2.left, gameData.minimumMatchNumber - 1, block.tag);
        }
        if (!matched)
        {
            IsMatchPossible(position, Vector2.right, gameData.minimumMatchNumber - 1, block.tag);
        }
    }

    bool IsMatchPossible(Vector2 origin, Vector2 direction, float distance, string blockTag)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, distance);

        if (!hits.All(blocks => blocks.transform.gameObject.CompareTag(blockTag) && hits.Length >= distance + 1))
        {
            return false;
        }

        List<GameObject> matchedBlocks = hits.Select(block => block.transform.gameObject).ToList();
        ClearCombo(matchedBlocks);

        return true;
    }

    void ClearCombo(List<GameObject> matchedBlocks)
    {
        OnMatch?.Invoke();
        PlayerInput.allowed = false;
        foreach (var block in matchedBlocks)
        {
            if (!block) continue;
            grid[(int)block.transform.position.y, (int)block.transform.position.x].active = false;
            block.GetComponent<Animator>().SetTrigger("OnDespawn");
        }
        GameManager.Get().AddScore(matchedBlocks.Count);
    }
    #endregion
}