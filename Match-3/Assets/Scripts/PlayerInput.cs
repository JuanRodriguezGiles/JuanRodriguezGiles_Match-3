using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PlayerInput : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] private LayerMask inputLayer;
    private List<GameObject> selectedBlocks;
    public static event Action<List<GameObject>> OnMouseReleased;
    public static event Action<GameObject> OnBlockSelected;
    public static event Action OnBlockDeselected;
    public static bool allowed;
    #endregion

    #region METHODS
    void OnEnable()
    {
        GameManager.OnGameOver += DisableInput;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= DisableInput;
    }

    void Start()
    {
        selectedBlocks = new List<GameObject>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && allowed)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D block = Physics2D.OverlapPoint(mousePosition, inputLayer);
            if (block)
            {
                TrySelectBlock(block.gameObject);
            }
        }
        else if (selectedBlocks.Count > 0)
        {
            OnMouseReleased?.Invoke(selectedBlocks);
        }
    }

    void TrySelectBlock(GameObject block)
    {
        if (selectedBlocks.Count == 0) //No active chain
        {
            selectedBlocks.Add(block);
            block.GetComponent<SpriteRenderer>().color = Color.green;
            block.GetComponent<Animator>().SetBool("Selected", true);
            OnBlockSelected?.Invoke(block.gameObject);
        }
        else if (IsSelectValid(block))
        {
            if (SelectedPreviousBlock(block))
            {
                selectedBlocks[selectedBlocks.Count - 1].GetComponent<SpriteRenderer>().color = Color.white;
                selectedBlocks[selectedBlocks.Count - 1].GetComponent<Animator>().SetBool("Selected", false);
                selectedBlocks.RemoveAt(selectedBlocks.Count - 1);
                OnBlockDeselected?.Invoke();
            }
            else if (!AlreadySelected(block))
            {
                selectedBlocks.Add(block);
                block.GetComponent<SpriteRenderer>().color = Color.green;
                block.GetComponent<Animator>().SetBool("Selected", true);
                OnBlockSelected?.Invoke(block.gameObject);
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

    void DisableInput()
    {
        allowed = false;
    }
    #endregion
}