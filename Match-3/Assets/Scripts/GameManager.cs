using UnityEngine;
public class GameManager : MonoBehaviour
{
    [Range(3, 8)] public int rows;
    [Range(3, 8)] public int columns;
    [SerializeField] private GameObject tilePrefab;
    public void Start()
    {
        CreateGrid();
        PositionCamera();
    }
    public void CreateGrid()
    {
        GameObject grid = new GameObject("Grid");
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector2 posistion = new Vector2(j, i);
                GameObject tile = Instantiate(tilePrefab, posistion, Quaternion.identity, grid.transform);
                tile.name = "Tile " + i +" "+ j;
            }
        }
    }
    public void PositionCamera()
    {
        Camera camera = FindObjectOfType<Camera>();
        float x = (float) rows / 2 - 0.5f;
        float y = (float) columns / 2 - 0.5f;
        Vector3 position = new Vector3(x, y, -100);
        camera.transform.position = position;
    }
}