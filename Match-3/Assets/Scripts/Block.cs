using UnityEngine;
public enum BLOCK_TYPES
{
    AIR,
    ARTHROPODA,
    DEMON,
    EARTH,
    FIRE,
    FLASH,
    FOREST,
    ICE,
    LINDWORM,
    SPIRIT,
    WATER
};
public class Block
{
    public BLOCK_TYPES type;
    public GameObject prefab;
    public void SetBlockType(int typeIndex)
    {
        BLOCK_TYPES currentType = (BLOCK_TYPES)typeIndex;
        type = currentType;
        prefab.tag = type.ToString();

        SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
        string path = "Art/Sprites/Blocks/" + type.ToString();
        renderer.sprite = Resources.Load<Sprite>(path);
    }
}