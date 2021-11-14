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
public class GameManager : MonoBehaviourSingleton<GameManager>
{
    #region PROPERTIES
    [Header("Game Setup")]
    [Range(6, 8)] public int rows;
    [Range(4, 6)] public int columns;
    [Range(2, 10)] public int minMatchNumber;
    [Range(1, 100)] [SerializeField] private int moves;
    public List<BLOCK_TYPES> blockTypes;
    public SPAWN_TYPES spawnType;
    [Range(0.01f, 0.1f)] public float spawnTime;

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
        movesLeft = moves;
        OnMovesChange?.Invoke(moves);
    }

    void Restart()
    {
        score = 0;
        OnScoreChange?.Invoke(0);
        movesLeft = moves;
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
        if (movesLeft != 0 && !PlayerInput.allowed) return;
        OnGameOver?.Invoke();
    }
    #endregion
}