using UnityEngine;
public class SoundtrackController : MonoBehaviour
{
    private int maxMoves;
    private AudioSource audio;
    public float percentage;
    void OnEnable()
    {
        audio = GetComponent<AudioSource>();
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
            audio.pitch = 1;
        }
        else if (percentage <= 25 || moves == 1)
        {
            audio.pitch = 1.25f;
        }
        else
        {
            audio.pitch = 1;
        }
    }
}