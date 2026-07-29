using UnityEngine;

public class OnDestroyPlaySound : MonoBehaviour
{
    public AudioClip soundEffect;
    public AudioSource audioSource;
    private void OnDestroy()
    {
        if (soundEffect != null)
        {
            audioSource.PlayOneShot(soundEffect);
        }
    }
}
