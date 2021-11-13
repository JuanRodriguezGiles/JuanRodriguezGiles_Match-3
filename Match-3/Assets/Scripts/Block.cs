using System;
using UnityEngine;
[Serializable]
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
[Serializable]
public class Block
{
    public BLOCK_TYPES type;
    public bool active = true;
    public int row;
    public int column;
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