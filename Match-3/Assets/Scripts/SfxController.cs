using UnityEngine;
public class SfxController : MonoBehaviourSingleton<SfxController>
{
    [SerializeField] private AudioClip select;
    [SerializeField] private AudioClip deselect;
    [SerializeField] private AudioClip match;
    [SerializeField] private AudioClip noMatch;
    private AudioSource audioSource;

    void OnEnable()
    {
        PlayerInput.OnBlockSelected += PlaySelectSound;
        PlayerInput.OnBlockDeselected += PlayDeselectSound;
        BoardManager.OnMatch += PlayMatchSound;
        BoardManager.OnNoMatch += PlayNoMatchSound;
    }
    void OnDisable()
    {
        PlayerInput.OnBlockSelected -= PlaySelectSound;
        PlayerInput.OnBlockDeselected -= PlayDeselectSound;
        BoardManager.OnMatch -= PlayMatchSound;
        BoardManager.OnNoMatch -= PlayNoMatchSound;

    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void PlaySelectSound(GameObject block)
    {
        audioSource.clip = select;
        audioSource.Play();
    }

    void PlayDeselectSound() //TODO play sound when only 1 block is deselected
    {
        audioSource.clip = deselect;
        audioSource.Play();
    }

    void PlayMatchSound()
    {
        audioSource.clip = match;
        audioSource.Play();
    }

    void PlayNoMatchSound()
    {
        audioSource.clip = noMatch;
        audioSource.Play();
    }
}