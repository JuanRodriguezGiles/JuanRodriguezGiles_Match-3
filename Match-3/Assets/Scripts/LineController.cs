using UnityEngine;
public class LineController : MonoBehaviour
{
    [SerializeField] private float width;
    private LineRenderer line;
    private int count;

    void OnEnable()
    {
        PlayerInput.OnBlockSelected += AddLine;
        PlayerInput.OnBlockDeselected += RemoveLine;
        BoardManager.OnClearBlocks += ClearLine;
    }

    void OnDisable()
    {
        PlayerInput.OnBlockSelected -= AddLine;
        PlayerInput.OnBlockDeselected -= RemoveLine;
        BoardManager.OnClearBlocks -= ClearLine;
    }

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.startWidth = width;
        line.endWidth = width;
        line.positionCount = 1;
    }

    void AddLine(GameObject block)
    {
        if (count > 0)
        {
            line.positionCount++;
        }
        line.SetPosition(count, block.transform.position);
        count++;
    }

    void RemoveLine()
    {
        line.positionCount--;
        count--;
    }

    void ClearLine()
    {
        line.positionCount = 0;
        line.positionCount++;
        count = 0;
    }
}