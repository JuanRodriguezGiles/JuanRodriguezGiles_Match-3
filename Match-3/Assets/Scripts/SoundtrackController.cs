using UnityEngine;
public class SoundtrackController : MonoBehaviour
{
    private int maxMoves;
    private AudioSource audioSource;
    public float percentage;
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

        percentage = ((float)moves / (float)maxMoves) * 100;
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
}