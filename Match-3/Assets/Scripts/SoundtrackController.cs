using UnityEngine;
public class SoundtrackController : MonoBehaviour
{
    #region PROPERTIES
    private int maxMoves;
    private AudioSource audioSource;
    private float percentage;
    #endregion

    #region METHODS
    void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        GameManager.OnMovesChange += ChangePitch;
    }

    void OnDisable()
    {
        GameManager.OnMovesChange -= ChangePitch;
    }

    void ChangePitch(int moves)
    {
        maxMoves = maxMoves == 0 ? moves : maxMoves;

        percentage = (moves / maxMoves) * 100;
        if (moves == 0)
        {
            audioSource.pitch = 1;
        }
        else if (percentage <= 25 || moves == 1)
        {
            audioSource.pitch = 1.25f;
        }
        else
        {
            audioSource.pitch = 1;
        }
    }
    #endregion
}