using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValBar : MonoBehaviour
{
    public Slider slider;
    public bool coroutineActive = false;

    public void SetMaxValue(float val)
    {
        slider.maxValue = val;
        slider.value = val;

    }

    public IEnumerator ChangeToVal(float val) //for smooth bar changes
    {
        coroutineActive = true;
        float preChangeVal = slider.value;
        float elapsed = 0f;
        float duration = 0.5f; // Duration of the change in seconds

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slider.value = Mathf.Lerp(preChangeVal, val, elapsed / duration);
            yield return null; // Wait for the next frame
        }
        slider.value = val; // Ensure the final value is set
        coroutineActive = false;
    }

    public void SetVal(float val) 
    {
        if (!coroutineActive) //don't interrupt bar transition
        {
            slider.value = val;
        }
    }
   

}
