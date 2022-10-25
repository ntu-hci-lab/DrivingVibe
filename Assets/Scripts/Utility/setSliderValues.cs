using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class setSliderValues : MonoBehaviour
{
    public Slider FreqSlider;
    public Text currentFreq;
    public Slider IntensitySlider;
    public Text currentIntensity;

    public float maxFreq;
    public float maxIntensity;


    private void Update()
    {
        currentFreq.text = Mathf.FloorToInt(maxFreq * FreqSlider.value + 10).ToString();
        currentIntensity.text = Mathf.FloorToInt(maxIntensity * IntensitySlider.value).ToString();
    }
}
