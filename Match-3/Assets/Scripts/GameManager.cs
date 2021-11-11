using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [Header("Game Setup")] [Range(3, 8)] public int rows;
    [Range(3, 8)] public int columns;
    public List<BLOCK_TYPES> blockTypes;
    [Range(2, 10)] public int minMatchNumber;
    [Header("Prefabs")] [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject blockPrefab;
    private GameObject blocks;
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
        BLOCK_TYPES[] previousLeft = new BLOCK_TYPES[rows];
        BLOCK_TYPES previousDown = BLOCK_TYPES.AIR;
        blocks = new GameObject("Blocks"); //Parent go for blocks

        for (int i = 0; i < rows; i++) //TODO tidy up code for pre match detection?
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(i, j);
                Block block = new Block();
                block.prefab = Instantiate(blockPrefab, position, Quaternion.identity, blocks.transform);
                block.prefab.name = "Block";

                List<BLOCK_TYPES> possibleCharacters = new List<BLOCK_TYPES>();
                possibleCharacters.AddRange(blockTypes);

                possibleCharacters.Remove(previousLeft[j]);
                possibleCharacters.Remove(previousDown);

                int type = (int)possibleCharacters[Random.Range(0, possibleCharacters.Count)];
                block.SetBlockType(type);

                previousLeft[j] = (BLOCK_TYPES)type;
                previousDown = (BLOCK_TYPES)type;
            }
        }
    }

    void ClearBlocks(List<GameObject> blocks)
    {
        if (blocks.Count >= minMatchNumber)
        {
            foreach (var block in blocks)
            {
                Destroy(block);
            }
            RefillGrid();
        }
        else
        {
            foreach (var block in blocks)
            {
                block.GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

        blocks.Clear();
    }

    void RefillGrid()
    {
        List<Vector2> emptyTilesPos = new List<Vector2>();
        for (int i = 0; i < rows; i++) //Find empty tiles
        {
            for (int j = 0; j < columns; j++)
            {
                Collider2D block = Physics2D.OverlapPoint(new Vector2(j, i));
                if (!block)
                {
                    emptyTilesPos.Add(new Vector2(j, i));
                }
            }
        }

        foreach (var position in emptyTilesPos)
        {
            Block block = new Block();
            block.prefab = Instantiate(blockPrefab, position, Quaternion.identity, blocks.transform);
            block.prefab.name = "Block";
            block.SetBlockType(Random.Range(0, blockTypes.Count));
        }
    }
}