using UnityEngine;
public class SfxController : MonoBehaviour
{
    #region PROPERTIES
    [SerializeField] private AudioClip select;
    [SerializeField] private AudioClip deselect;
    [SerializeField] private AudioClip match;
    [SerializeField] private AudioClip noMatch;
    private AudioSource audioSource;
    #endregion

    #region METHODS
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
        audioSource.pitch = 1;
        audioSource.Play();
    }

    void PlayDeselectSound()
    {
        audioSource.clip = deselect;
        audioSource.pitch = 2;
        audioSource.Play();
    }

    void PlayMatchSound()
    {
        audioSource.clip = match;
        audioSource.pitch = 1;
        audioSource.Play();
    }

    void PlayNoMatchSound()
    {
        audioSource.clip = noMatch;
        audioSource.pitch = 1;
        audioSource.Play();
    }
    #endregion
}