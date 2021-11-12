using TMPro;
using UnityEngine;
public class UiGameplay : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text movesLeftText;
    private TMP_Text versionText;

    void OnEnable()
    {
        GameManager.OnScoreChange += UpdateScoreText;
        GameManager.OnMovesChange += UpdateMovesLeftText;
    }

    void OnDisable()
    {
        GameManager.OnScoreChange -= UpdateScoreText;
        GameManager.OnMovesChange -= UpdateMovesLeftText;
    }

    void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score: " + newScore;
    }

    void UpdateMovesLeftText(int newMoves)
    {
        movesLeftText.text = "Moves: " + newMoves;
    }
}