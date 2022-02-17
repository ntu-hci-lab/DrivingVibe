using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class VirtualHeadband : MonoBehaviour
{
    private bool writeEnable = false;
    public string calibrationFilePath = @"D:\calibrationResults\";
    public string fileName = "0.txt";
    public string vibrationRecordPath = @"D:\vibrateRecords\";
    private string vibrationRecordName = "record.txt";
    public bool isRandom;
    public int UserID = 0;

    // From 0~100
    public int[] VibratorFrequencies = new int[16];

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
    private int maxiIntensity;
    private int cueMaxIntensity;
    private int motionnMaxIntensity;
    public int maxValue;
    public bool isMotion;

    // get from nowhere
    public int defaultFreq = 150;
    public int minFreq = 10;
    public int maxFreq = 250;
    public float updateInterval = 0.05f;

    StreamWriter sw;
    FileInfo vibrationRecordFile;
    private string writeLine = "";
    private float sampleInterval = 0.2f;

    private ArduinoSystem arduinoSystem;
        
    private byte[] toArduinoBytes = new byte[3];

    // Start is called before the first frame update
    void Start()
    {
        if (isRandom)
        {
            vibrationRecordName = UserID.ToString() + "_random.txt";
        }
        else
        {
            vibrationRecordName = UserID.ToString() + "_pattern.txt";
        }
        // additionalIntensityEnable = false;
        isMotion = false;
        Iniitialize();
        arduinoSystem = gameObject.GetComponent<ArduinoSystem>();
        StartCoroutine(sendHeadbandStateToArduino());

        try
        {
            if (!Directory.Exists(vibrationRecordPath))
            {
                Directory.CreateDirectory(vibrationRecordPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        vibrationRecordFile = new FileInfo(Path.Combine(vibrationRecordPath, vibrationRecordName));

        //StreamWriter sw = new StreamWriter(vibrationRecordPath, false);
        
        sw = vibrationRecordFile.CreateText();
        StartCoroutine(recordParameters());
    }

    private IEnumerator recordParameters()
    {
        Vector2 planarG = GetComponent<PatternGenerator>().planarGforce;
        float[] suspensionDiff = new float[4];
        suspensionDiff = GetComponent<additionalIntensityGenerator>().suspensionDiff;
        writeLine = planarG.x.ToString() + " " + planarG.y.ToString();
        sw.WriteLine(writeLine);
        writeLine = suspensionDiff[0] + " " + suspensionDiff[1] + " " + suspensionDiff[2] + " " + suspensionDiff[3];
        sw.WriteLine(writeLine);
        yield return new WaitForSeconds(sampleInterval);
        StartCoroutine(recordParameters());
        yield break;
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
            VibratorFrequencies[i] = 58;
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
            VibratorFrequencies[i] = 58;
            VibratorLifeSpans[i] = 0.0f;
        }
    }

    private IEnumerator sendHeadbandStateToArduino()
    {
        int intTmp = 0;
        int freqTmp = 100;
        for (int i = 0; i < 16; i++)
        {
            if (VibratorLifeSpans[i] > 0 || VibratorLifeSpans[i] <= 0)
            {
                intTmp = 0;
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

                if(VibratorMotionIntensities[i] > 0 && VibratorLifeSpans[i] > 0)
                {
                    intTmp += (0 + (motionnMaxIntensity - 0) * VibratorMotionIntensities[i] / 100) * VibratorIntensityWeight[i] / 100;
                }

                if (VibratorAdditionalIntensities[i] > 0 && VibratorAddiLifeSpans[i] > 0)
                {
                    intTmp += VibratorAdditionalIntensities[i] * VibratorIntensityWeight[i] / 100;
                }

                intTmp = Mathf.Min(110, intTmp);

                // translate the percentile frequency to real value
                if (VibratorFrequencies[i] > 0)
                {
                    // freqTmp = minFreq + ((maxFreq - minFreq) * VibratorFrequencies[i] / 100);
                    freqTmp = 170;
                }
                else
                {
                    // 0 means least amount of frequency
                    freqTmp = 170;
                }

                // freqTmp = defaultFreq; // comment this if freq need to be mapping
                // Debug.Log(intTmp);
                toArduinoBytes[0] = System.Convert.ToByte((char)i);
                toArduinoBytes[1] = System.Convert.ToByte((char)(freqTmp / 2));
                toArduinoBytes[2] = System.Convert.ToByte((char)intTmp);
                arduinoSystem.writeToArduinoByte(toArduinoBytes);


                VibratorLifeSpans[i] -= updateInterval;
                VibratorAddiLifeSpans[i] -= updateInterval;
            }
            else
            {
                toArduinoBytes[0] = System.Convert.ToByte((char)i);
                toArduinoBytes[1] = System.Convert.ToByte((char)(freqTmp / 2));
                toArduinoBytes[2] = System.Convert.ToByte((char)0);
                arduinoSystem.writeToArduinoByte(toArduinoBytes);
                if(writeEnable){
                    writeLine = toArduinoBytes[0].ToString() + " " + toArduinoBytes[1].ToString() + " " + toArduinoBytes[2].ToString();
                    sw.WriteLine(writeLine);
                }
            }
        }

        yield return new WaitForSeconds(updateInterval);
        // yield return 0;
        StartCoroutine(sendHeadbandStateToArduino());
        yield break;
    }
    private void OnApplicationQuit()
    {
        sw.Close();
    }
}
