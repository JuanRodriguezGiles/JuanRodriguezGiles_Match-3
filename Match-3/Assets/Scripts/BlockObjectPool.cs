using UnityEngine;
using UnityEngine.Pool;
public class BlockObjectPool : MonoBehaviourSingleton<BlockObjectPool>
{
    #region PROPERTIES
    [SerializeField] private GameObject blockPrefab;
    private bool collectionChecks = true;
    private int poolSize;
    private IObjectPool<GameObject> pool;

    public IObjectPool<GameObject> Pool
    {
        get
        {
            if (pool == null)
            {
                poolSize = GameManager.Get().gameData.rows * GameManager.Get().gameData.columns;

                pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                    OnDestroyPoolObject, collectionChecks, poolSize, poolSize);
            }
            return pool;
        }
    }
    #endregion

    #region METHODS
    GameObject CreatePooledItem()
    {
        GameObject block = Instantiate(blockPrefab);

        return block;
    }

    void OnReturnedToPool(GameObject block)
    {
        block.GetComponent<Collider2D>().enabled = true;
        block.GetComponent<SpriteRenderer>().color = Color.white;
        block.GetComponent<Animator>().SetBool("Selected", false);
        block.transform.localScale = Vector3.one;
        block.SetActive(false);
    }

    void OnTakeFromPool(GameObject block)
    {
        block.SetActive(true);
    }

    void OnDestroyPoolObject(GameObject block)
    {
        Destroy(block.gameObject);
    }
    #endregion
}