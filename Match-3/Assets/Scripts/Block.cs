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
    public GameObject blockPrefab;
    public BLOCK_TYPES type;
}