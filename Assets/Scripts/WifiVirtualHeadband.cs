using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class WifiVirtualHeadband : MonoBehaviour
{
private bool writeEnable = false;
    public string calibrationFilePath = @"D:\calibrationResults\";
    public string fileName = "0.txt";
    public bool isRandom;
    public int UserID = 0;

    // From 0~80 intensity
    public int[] VibratorIntensities = new int[16];

    // From 0~40 intensity
    public int[] VibratorMotionIntensities = new int[16];

    // From 0~40 intensity
    public int[] VibratorAdditionalIntensities = new int[16];
    public bool additionalIntensityEnable;

    private int[] VibratorIntensityWeight = new int[16];

    // For tactile motion
    public float[] VibratorLifeSpans = new float[16];

    // For Rumbling
    public float[] VibratorAddiLifeSpans = new float[16];

    // get from calibration
    private int maxIntensity;
    private int cueMaxIntensity;
    private int motionnMaxIntensity;
    public int maxValue;
    public bool isMotion;

    public float updateInterval = 0.05f;

    StreamWriter sw;

    private WifiToArduino arduinoSystem;
        
    private byte[] toArduinoBytes = new byte[16];

    // Start is called before the first frame update
    void Start()
    {
        isMotion = false;
        Iniitialize();
        arduinoSystem = gameObject.GetComponent<WifiToArduino>();
        //StartCoroutine(sendHeadbandStateToArduino());
    }

    void Iniitialize()
    {
        FileInfo calibrationFile = new FileInfo(calibrationFilePath + fileName);
        StreamReader reader = calibrationFile.OpenText();
        for (int i = 0; i < 16; i++)
        {
            VibratorIntensityWeight[i] = int.Parse(reader.ReadLine());
            VibratorIntensities[i] = 0;
            VibratorMotionIntensities[i] = 0;
            VibratorAdditionalIntensities[i] = 0;
            VibratorLifeSpans[i] = 0.0f;
            VibratorAddiLifeSpans[i] = 0.0f;
        }
        motionnMaxIntensity = maxValue / 2; // int.Parse(reader.ReadLine());
        cueMaxIntensity = maxValue; //int.Parse(reader.ReadLine());
        reader.Close();
    }

    public void setAllToZero()
    {
        for (int i = 0; i < 16; i++)
        {
            VibratorIntensities[i] = 0;
            VibratorMotionIntensities[i] = 0;
            VibratorLifeSpans[i] = 0.0f;
        }
    }

    public IEnumerator sendHeadbandStateToArduino()
    {
        int intTmp = 0;
        for (int i = 0; i < 16; i++)
        {
            // translate the percentile intensity to real value
            if (VibratorIntensities[i] > 0)
            {
                intTmp = (0 + (cueMaxIntensity - 0) * VibratorIntensities[i] / 100) * VibratorIntensityWeight[i] / 100;
            }
            else
            {
                // 0 means no vibration
                intTmp = 0;
            }

            if (VibratorMotionIntensities[i] > 0 && VibratorLifeSpans[i] > 0)
            {
                intTmp += (0 + (motionnMaxIntensity - 0) * VibratorMotionIntensities[i] / 100) * VibratorIntensityWeight[i] / 100;
            }

            if (VibratorAdditionalIntensities[i] > 0 && VibratorAddiLifeSpans[i] > 0)
            {
                intTmp += VibratorAdditionalIntensities[i] * VibratorIntensityWeight[i] / 100;
            }

            intTmp = Mathf.Min(110, intTmp);

            toArduinoBytes[i] = System.Convert.ToByte((char)intTmp);

            VibratorLifeSpans[i] -= updateInterval;
            VibratorAddiLifeSpans[i] -= updateInterval;
        }
        arduinoSystem.writeToArduinoByte(toArduinoBytes);
        Debug.Log("Just write something!!");
        yield return new WaitForSeconds(updateInterval);
        // yield return 0;
        StartCoroutine(sendHeadbandStateToArduino());
        yield break;
    }

}
