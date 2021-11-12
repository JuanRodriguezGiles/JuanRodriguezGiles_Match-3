using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public class GameManager : MonoBehaviour
{
    [Header("Game Setup")]
    [Range(3, 8)] public int rows;
    [Range(3, 8)] public int columns;
    [Range(2, 10)] public int minMatchNumber;
    [Range(1, 100)] public int moves;
    public List<BLOCK_TYPES> blockTypes;

    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject blockPrefab;

    private GameObject blocks;
    private GameObject[,] grid;
    private int score = 0;

    public static event Action<int> OnScoreChange;
    public static event Action<int> OnMovesChange;

    void OnEnable()
    {
        PlayerInput.OnMouseReleased += ClearBlocks;
    }

    void OnDisable()
    {
        PlayerInput.OnMouseReleased -= ClearBlocks;
    }

    void Start()
    {
        CreateGrid();
        CenterCameraOnGrid();
        StartCoroutine(InstantiateBlocks());
        OnMovesChange?.Invoke(moves);
    }

    void CreateGrid()
    {
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

    void CenterCameraOnGrid()
    {
        Camera camera = FindObjectOfType<Camera>();
        float x = (float)rows / 2 - 0.5f;
        float y = (float)columns / 2 - 0.5f;
        Vector3 position = new Vector3(x, y, -100);
        camera.transform.position = position;
    }

    IEnumerator InstantiateBlocks()
    {
        grid = new GameObject[rows, columns];
        BLOCK_TYPES[] previousLeft = new BLOCK_TYPES[rows];
        BLOCK_TYPES previousDown = BLOCK_TYPES.AIR;
        blocks = new GameObject("Blocks"); //Parent go for blocks

        for (int i = 0; i < rows; i++) //TODO tidy up code for pre match detection?
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(j, i);
                Block block = new Block();
                block.prefab = Instantiate(blockPrefab, position, Quaternion.identity, blocks.transform);
                block.prefab.name = "Block";

                List<BLOCK_TYPES> possibleBlocks = new List<BLOCK_TYPES>();
                possibleBlocks.AddRange(blockTypes);

                possibleBlocks.Remove(previousLeft[j]);
                possibleBlocks.Remove(previousDown);

                int type = (int)possibleBlocks[Random.Range(0, possibleBlocks.Count)];
                block.SetBlockType(type);

                previousLeft[j] = (BLOCK_TYPES)type;
                previousDown = (BLOCK_TYPES)type;

                grid[i, j] = block.prefab;
                yield return new WaitForSeconds(0.05f);
            }
        }
        PlayerInput.allowed = true;
    }

    void ClearBlocks(List<GameObject> selectedBlocks)
    {
        if (selectedBlocks.Count >= minMatchNumber)
        {
            PlayerInput.allowed = false;
            foreach (var block in selectedBlocks)
            {
                if (!block) continue;
                block.GetComponent<Animator>().SetTrigger("OnDespawn");
                Destroy(block.gameObject, 1);
            }
            AddScore(selectedBlocks.Count);
            UpdateMovesLeft();
            StartCoroutine(RefillGrid());
        }
        else
        {
            foreach (var block in selectedBlocks)
            {
                block.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }
        selectedBlocks.Clear();
    }

    void ClearCombo(List<GameObject> matchedBlocks)
    {
        PlayerInput.allowed = false;
        foreach (var block in matchedBlocks)
        {
            if (!block) continue;
            block.GetComponent<Animator>().SetTrigger("OnDespawn");
            Destroy(block.gameObject, 1);
        }
        AddScore(matchedBlocks.Count);
        StartCoroutine(RefillGrid());
    }

    void AddScore(int blocksDestroyed)
    {
        score += blocksDestroyed * 10;
        OnScoreChange?.Invoke(score);
    }

    void UpdateMovesLeft()
    {
        moves--;
        OnMovesChange?.Invoke(moves);
    }

    IEnumerator RefillGrid()
    {
        yield return new WaitForSeconds(1);
        List<Vector2> emptyTilesPos = GetEmptyTiles();
        foreach (var position in emptyTilesPos)
        {
            Block block = new Block();
            block.prefab = Instantiate(blockPrefab, position, Quaternion.identity, blocks.transform);
            block.prefab.name = "Block";
            block.SetBlockType(Random.Range(0, blockTypes.Count));
        }
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Collider2D block = Physics2D.OverlapPoint(new Vector2(j, i));
                if (block)
                {
                    CheckMatches(block.gameObject);
                }
            }
        }
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

    IEnumerator DropBlock(GameObject block, float targetY)
    {
        float speed = 1;
        while (block.transform.position.y > targetY)
        {
            block.transform.position -= speed * Time.deltaTime * new Vector3(0, 1, 0);
            yield return null;
        }
    }

    void CheckMatches(GameObject block)
    {
        List<GameObject> matchedBlocks = new List<GameObject>();
        bool matched = false;
        Vector2 pos = new Vector2(block.transform.position.x, block.transform.position.y);
        RaycastHit2D[] hitUp = Physics2D.RaycastAll(pos, Vector2.up, minMatchNumber - 1);
        RaycastHit2D[] hitDown = Physics2D.RaycastAll(pos, Vector2.down, minMatchNumber - 1);
        RaycastHit2D[] hitLeft = Physics2D.RaycastAll(pos, Vector2.left, minMatchNumber - 1);
        RaycastHit2D[] hitRight = Physics2D.RaycastAll(pos, Vector2.right, minMatchNumber - 1);

        if (hitUp.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag) && hitUp.Length >= minMatchNumber))
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
        PlayerInput.allowed = true;
    }

    void GetGrid()
    {
        Vector2 pos;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                pos = new Vector2(j, i);
                Collider2D block = Physics2D.OverlapPoint(pos);
                if (block)
                {
                    grid[i, j] = block.gameObject;
                }
                else
                {
                    grid[i, j] = null;
                }
            }
        }
    }
}