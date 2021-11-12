using UnityEngine;
public class SfxController : MonoBehaviourSingleton<SfxController>
{
    [SerializeField] private AudioClip select;
    [SerializeField] private AudioClip deselect;
    [SerializeField] private AudioClip match;
    [SerializeField] private AudioClip noMatch;
    private AudioSource audio;

    void OnEnable()
    {
        PlayerInput.OnBlockSelected += PlaySelectSound;
        PlayerInput.OnBlockDeselected += PlayDeselectSound;
        GameManager.OnMatch += PlayMatchSound;
        GameManager.OnNoMatch += PlayNoMatchSound;
    }
    void OnDisable()
    {
        PlayerInput.OnBlockSelected -= PlaySelectSound;
        PlayerInput.OnBlockDeselected -= PlayDeselectSound;
        GameManager.OnMatch -= PlayMatchSound;
        GameManager.OnNoMatch -= PlayNoMatchSound;

    }

    void Start()
    {
        audio = GetComponent<AudioSource>();
    }

    void PlaySelectSound(GameObject block)
    {
        audio.clip = select;
        audio.Play();
    }

    void PlayDeselectSound() //TODO play sound when only 1 block is deselected
    {
        audio.clip = deselect;
        audio.Play();
    }

    void PlayMatchSound()
    {
        audio.clip = match;
        audio.Play();
    }

    void PlayNoMatchSound()
    {
        audio.clip = noMatch;
        audio.Play();
    }
}