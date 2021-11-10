using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
public class GameManager : MonoBehaviour
{
    [Header("Grid Setup")]
    [Range(3, 8)] public int rows;
    [Range(3, 8)] public int columns;
    [Header("Prefabs")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject blockPrefab;
    [Header("Block Types")] public List<BLOCK_TYPES> types;
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
        GameObject blocks = new GameObject("Blocks");
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 position = new Vector2(j, i);
                Block block = new Block();
                block.prefab = Instantiate(blockPrefab, position, Quaternion.identity, blocks.transform);
                block.prefab.name = "Block " + i + " " + j;
                block.SetBlockType(Random.Range(0, types.Count));
            }
        }
    }
}