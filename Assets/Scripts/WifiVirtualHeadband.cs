using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class WifiVirtualHeadband : VirtualLayer
{

    private WifiToArduino WifiArduinoSystem;

    private byte[] WifiToArduinoBytes = new byte[16];

    private void Start()
    {
        WifiArduinoSystem = gameObject.GetComponent<WifiToArduino>();
        motionnMaxIntensity = maxValue / 2; // int.Parse(reader.ReadLine());
        cueMaxIntensity = maxValue; //int.Parse(reader.ReadLine());
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
            intTmp = 40;
            toArduinoBytes[i] = System.Convert.ToByte((char)intTmp);

            VibratorLifeSpans[i] -= updateInterval;
            VibratorAddiLifeSpans[i] -= updateInterval;
        }
        WifiArduinoSystem.writeToArduinoByte(toArduinoBytes);
        yield return new WaitForSeconds(updateInterval);
        // yield return 0;
        StartCoroutine(sendHeadbandStateToArduino());
        yield break;
    }

    public override void StartMonitor()
    {
        StartCoroutine(sendHeadbandStateToArduino());
    }

}
