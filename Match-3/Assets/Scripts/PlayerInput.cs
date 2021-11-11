using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private LayerMask inputLayer;
    private List<GameObject> selectedBlocks;
    public static event Action<List<GameObject>> OnMouseReleased;
    void Start()
    {
        selectedBlocks = new List<GameObject>();
    }
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D block = Physics2D.OverlapPoint(mousePosition, inputLayer);
            if (block)
            {
                SelectBlock(block.gameObject);
            }
        }
        else
        {
            OnMouseReleased?.Invoke(selectedBlocks);
        }
    }
    void SelectBlock(GameObject block)
    {
        if (selectedBlocks.Count == 0)
        {
            selectedBlocks.Add(block);
            block.GetComponent<SpriteRenderer>().color = Color.green; //TODO flashing animation?
        }
        else if (IsSelectValid(block))
        {
            if (SelectedPreviousBlock(block))
            {
                selectedBlocks[selectedBlocks.Count - 1].gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                selectedBlocks.RemoveAt(selectedBlocks.Count - 1);
            }
            else if (!AlreadySelected(block))
            {
                selectedBlocks.Add(block);
                block.GetComponent<SpriteRenderer>().color = Color.green; //TODO flashing animation?
            }
        }
    }
    bool IsSelectValid(GameObject block)
    {
        GameObject lastBlock = selectedBlocks[selectedBlocks.Count - 1];
        Vector2 position = lastBlock.transform.position;

        Collider2D[] hits;
        hits = Physics2D.OverlapBoxAll(position, new Vector2(2, 2), 0, inputLayer);

        return hits.Any(blocks => blocks.gameObject.GetInstanceID() == block.GetInstanceID() && blocks.gameObject.CompareTag(lastBlock.tag));
    }
    bool SelectedPreviousBlock(GameObject block)
    {
        if (selectedBlocks.Count < 2)
        {
            return false;
        }
        GameObject lastBlock = selectedBlocks[selectedBlocks.Count - 2];
        return lastBlock.GetInstanceID() == block.GetInstanceID();
    }
    bool AlreadySelected(GameObject block)
    {
        return block.GetComponent<SpriteRenderer>().color == Color.green; //TODO check with "selected" boolean?
    }
}