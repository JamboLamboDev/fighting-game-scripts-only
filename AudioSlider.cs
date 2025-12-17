using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
public class AudioSlider : MonoBehaviour
{
    public AudioMixer masterMixer;
    public string childMixerName; //in prefab for slider ex... Master Volume or BGM Volume
    
    public void SetVolume(float volume)
    {
        masterMixer.SetFloat(childMixerName, Mathf.Log10(volume) * 20);
    }
    
}
