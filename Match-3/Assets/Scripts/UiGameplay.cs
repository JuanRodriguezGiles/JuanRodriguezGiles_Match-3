using System;
using TMPro;
using UnityEngine;
public class UiGameplay : MonoBehaviour
{
    #region PROPERTIES
    public TMP_Text scoreText;
    public TMP_Text movesLeftText;
    public TMP_Text versionText;
    public RectTransform gameOverPanel;
    public static event Action OnPlayButtonPressed;
    #endregion

    #region METHODS
    void OnEnable()
    {
        GameManager.OnScoreChange += UpdateScoreText;
        GameManager.OnMovesChange += UpdateMovesLeftText;
        GameManager.OnGameOver += OnGameOver;

        versionText.text = "v" + Application.version + " - Juan Rodriguez Giles";
        gameOverPanel.gameObject.SetActive(false);
    }
    void OnDisable()
    {
        GameManager.OnScoreChange -= UpdateScoreText;
        GameManager.OnMovesChange -= UpdateMovesLeftText;
        GameManager.OnGameOver -= OnGameOver;
    }

    void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    void UpdateMovesLeftText(int newMoves)
    {
        movesLeftText.text = "Moves: " + newMoves;
    }
    void OnGameOver()
    {
        gameOverPanel.gameObject.SetActive(true);
    }

    public void PlayButtonPressed()
    {
        gameOverPanel.gameObject.SetActive(false);
        OnPlayButtonPressed?.Invoke();
    }
    #endregion
}