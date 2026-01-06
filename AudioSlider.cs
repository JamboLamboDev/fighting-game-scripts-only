using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.UI;


public class AudioSlider : MonoBehaviour
{
    public AudioMixer masterMixer;
    private float currentVolume;
    public string childMixerName; //in prefab for slider ex... Master Volume or BGM Volume
    public Slider slider; //this slider component
    public void Awake()
    {
        slider = GetComponentInChildren<Slider>();     
    }
    public void OnEnable()
    {
        currentVolume = PlayerPrefs.GetFloat(childMixerName, 1f); // default at max volume, otherwise remember previous setting.
        slider.value = currentVolume;
        masterMixer.SetFloat(childMixerName, Mathf.Log10(currentVolume) * 20);

    }
    
    public void SetVolume(float volume)//called when slider is changed or at start, adjusts volume of child mixer and saves 
    {

        currentVolume = volume;
        masterMixer.SetFloat(childMixerName, Mathf.Log10(currentVolume) * 20);
        PlayerPrefs.SetFloat(childMixerName, currentVolume);
        PlayerPrefs.Save();
        Debug.Log($"Saved Value: {PlayerPrefs.GetFloat(childMixerName)}");
        Debug.Log($"SAVING key='{childMixerName}' value={volume}");
    }
    
}
