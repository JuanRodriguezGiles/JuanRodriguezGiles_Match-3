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
    public List<BLOCK_TYPES> blockTypes;

    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject blockPrefab;

    private GameObject blocks;
    [SerializeField] public GameObject[,] grid;
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
        InstantiateBlocks();
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

    void InstantiateBlocks()
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
            }
        }
    }

    void ClearBlocks(List<GameObject> selectedBlocks)
    {
        if (selectedBlocks.Count >= minMatchNumber)
        {
            GetGrid();
            foreach (var _block in selectedBlocks)
            {
                if (_block)
                {
                    int x = (int)_block.transform.position.x;
                    int y = (int)_block.transform.position.y;
                    grid[y, x] = null;
                    Destroy(_block.gameObject);
                }
            }
            RefillGrid();
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

    void RefillGrid()
    {
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
        Vector2 pos = new Vector2(block.transform.position.x, block.transform.position.y);
        RaycastHit2D[] hitUp = Physics2D.RaycastAll(pos, Vector2.up, minMatchNumber-1);
        RaycastHit2D[] hitDown = Physics2D.RaycastAll(pos, Vector2.down, minMatchNumber-1);
        RaycastHit2D[] hitLeft = Physics2D.RaycastAll(pos, Vector2.left, minMatchNumber-1);
        RaycastHit2D[] hitRight = Physics2D.RaycastAll(pos, Vector2.right, minMatchNumber-1);

        if (hitUp.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag)&&hitUp.Length>=minMatchNumber))
        {
            Debug.Log("matchup");
        }
        if (hitDown.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag) && hitDown.Length >= minMatchNumber))
        {
            Debug.Log("matchdown");
        }
        if (hitLeft.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag) && hitLeft.Length >= minMatchNumber))
        {
            Debug.Log("matchleft");
        }
        if (hitRight.All(_blocks => _blocks.transform.gameObject.CompareTag(block.tag) && hitRight.Length >= minMatchNumber))
        {
            Debug.Log("matchright");
        }
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