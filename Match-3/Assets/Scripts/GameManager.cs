using System;
using System.Collections.Generic;
using UnityEngine;
public enum SPAWN_TYPES
{
    LEFT_RIGHT_UP,
    RIGHT_LEFT_UP,
    LEFT_RIGHT_DOWN,
    RIGHT_LEFT_DOWN
}
[Serializable]
public struct GameData
{
    [Range(6, 8)] public int rows;
    [Range(4, 6)] public int columns;
    [Range(2, 10)] public int minimumMatchNumber;
    [Range(1, 100)] public int moves;
    public List<BLOCK_TYPES> blockTypes;
    public SPAWN_TYPES spawnType;
    [Range(0.01f, 0.1f)] public float spawnTime;
}
public class GameManager : MonoBehaviourSingleton<GameManager>
{
    #region PROPERTIES
    public GameData gameData;

    private int score;
    private int movesLeft;

    public static event Action<int> OnScoreChange;
    public static event Action<int> OnMovesChange;
    public static event Action OnGameOver;
    #endregion

    #region METHODS
    void OnEnable()
    {
        UiGameplay.OnPlayButtonPressed += Restart;
    }

    void OnDisable()
    {
        UiGameplay.OnPlayButtonPressed -= Restart;
    }

    void Start()
    {
        movesLeft = gameData.moves;
        OnMovesChange?.Invoke(movesLeft);
    }

    void Restart()
    {
        score = 0;
        OnScoreChange?.Invoke(0);
        movesLeft = gameData.moves;
        OnMovesChange?.Invoke(movesLeft);
    }

    public void AddScore(int blocksDestroyed)
    {
        score += blocksDestroyed * 10;
        OnScoreChange?.Invoke(score);
    }

    public void UpdateMovesLeft()
    {
        movesLeft--;
        OnMovesChange?.Invoke(movesLeft);
    }

    public void CheckForGameOver()
    {
        if (movesLeft == 0 && PlayerInput.allowed)
        {
            OnGameOver?.Invoke();
        }
    }
    #endregion
}