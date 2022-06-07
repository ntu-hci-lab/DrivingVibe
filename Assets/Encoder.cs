using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class Encoder : MonoBehaviour
{
    private float PowerSource = 9.0f;
    private float ForcePerVoltage = 1.6f;
    private float MaximumForce = 2.3f;
    public bool useWifi = true;
    public string calibrationFilePath = @"D:\calibrationResults\";
    public string fileName = "0.txt";
    public bool isRandom;
    public int UserID = 0;

    [SerializeField]
    protected int[] VibratorIntensityWeight = new int[16];

    // get from calibration
    protected int maxIntensity;
    private int SystemInputMaxValue;
    public int minValue = 8;
    public int maxValue;
    public int globalMultiplier = 100;

    // public float updateInterval = 0.05f;
    public int FramesPerUpdate = 2;

    protected VirtualHeadband virtualHeadband;
    protected ShowVibration showVibration;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        showVibration = GetComponent<ShowVibration>();
        virtualHeadband = GetComponent<VirtualHeadband>();
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
        }
        SystemInputMaxValue = Mathf.FloorToInt(((MaximumForce / ForcePerVoltage) / PowerSource) * 256);
        // maxValue = Mathf.Min(int.Parse(reader.ReadLine()), SystemInputMaxValue); // Get from calibration
        globalMultiplier = int.Parse(reader.ReadLine()); // Get from calibration
        maxValue = SystemInputMaxValue;
        reader.Close();
        Debug.Log("Initialization finished.");
    }
    public virtual void SendDataToArduino(byte[] data)
    {
        // currently unused
    }
}
