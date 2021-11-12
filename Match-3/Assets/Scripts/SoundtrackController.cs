using UnityEngine;
public class SoundtrackController : MonoBehaviour
{
    private int maxMoves;
    private AudioSource audio;
    public float percentage;
    void OnEnable()
    {
        GameManager.OnMovesChange += ChangePitch;
    }

    void OnDisable()
    {
        GameManager.OnMovesChange -= ChangePitch;
    }

    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    void ChangePitch(int moves)
    {
        maxMoves = maxMoves == 0 ? moves : maxMoves;

        percentage = ((float)moves / (float)maxMoves) * 100;
        if (percentage <= 25)
        {
            audio.pitch = 1.25f;
        }
        else
        {
            audio.pitch = 1;
        }
    }
}