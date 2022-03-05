using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class VirtualLayer : MonoBehaviour
{
    public bool useWifi = true;
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

    protected int[] VibratorIntensityWeight = new int[16];

    // For tactile motion
    public float[] VibratorLifeSpans = new float[16];

    // For Rumbling
    public float[] VibratorAddiLifeSpans = new float[16];

    // get from calibration
    protected int maxIntensity;
    protected int cueMaxIntensity;
    protected int motionnMaxIntensity;
    public int maxValue;
    public bool isMotion;

    public float updateInterval = 0.05f;

    StreamWriter sw;

    private ArduinoSystem ArduinoSystem;

    protected byte[] toArduinoBytes = new byte[16];

    // Start is called before the first frame update
    void Start()
    {
        isMotion = false;
        Iniitialize();
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

    public virtual void StartMonitor()
    {

    }

    public virtual void SendDataToArduino(byte[] data)
    {

    }
}
