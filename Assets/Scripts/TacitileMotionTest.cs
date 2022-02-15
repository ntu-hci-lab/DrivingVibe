using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacitileMotionTest : MonoBehaviour
{
    public ArduinoSystem arduinoSystem;
    private byte[] arduinoData = new byte[3];

    public Slider FreqSlider;
    public float maxFreq;
    public Slider IntensitySlider;
    public float maxIntensity;
    public Dropdown vibratorIndex;

    private int frequency = 150;
    private int intensity = 150;

    // vibrator interval : 30 degree
    // token speed = 30 / ISOI (degree)
    // token range = speed * duration
    // token start pos = 1st vibrator pos +- 0.5 * range

    private void Start()
    {
        arduinoSystem = gameObject.GetComponent<ArduinoSystem>();
        frequency = Mathf.FloorToInt(maxFreq * FreqSlider.value + 10);
        intensity = Mathf.FloorToInt(maxIntensity * IntensitySlider.value);
    }

    private void setAllToZero()
    {
        arduinoSystem.setAllToZero();
    }

    public void sendByte(int condition)
    {
        int index = vibratorIndex.value;
        frequency = Mathf.FloorToInt(maxFreq * FreqSlider.value + 10);

        if(condition == 0)
        {
            intensity = Mathf.FloorToInt(maxIntensity * IntensitySlider.value);
        }
        else
        {
            intensity = 0;
        }

        arduinoData[0] = System.Convert.ToByte((char)index);
        arduinoData[1] = System.Convert.ToByte((char)(frequency / 2));
        arduinoData[2] = System.Convert.ToByte((char)intensity);
        arduinoSystem.writeToArduinoByte(arduinoData);
    }

}



