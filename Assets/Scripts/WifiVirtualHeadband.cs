using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class WifiVirtualHeadband : VirtualLayer
{

    private WifiToArduino Arduino;
    private bool isMonitorOn = false;
    private byte[] WifiToArduinoBytes = new byte[16];
    private Coroutine currentMonitor;

    protected override void Start()
    {
        base.Start();
        Arduino = gameObject.GetComponent<WifiToArduino>();
        if (!Arduino.arduinoPaused)
        {
            // arduino on, start sending
            currentMonitor = StartCoroutine(sendHeadbandStateToArduino());
            isMonitorOn = true;
        }
    }

    private void Update()
    {
        if (Arduino.arduinoPaused && currentMonitor != null)
        {
            // arduino off, stop sending
            Debug.Log("Ardiono paused, stop sending headband state!");
            StopAllCoroutines();
            isMonitorOn = false;
        }
        if (!isMonitorOn && !Arduino.arduinoPaused)
        {
            // arduino on, start sending
            Debug.Log("Ardiono on, (restart) sending headband state!");
            currentMonitor = StartCoroutine(sendHeadbandStateToArduino());
            isMonitorOn = true;
        }
    }

    public IEnumerator sendHeadbandStateToArduino()
    {
        int intTmp = 0;
        for (int i = 0; i < 16; i++)
        {
            intTmp = 0;
            // Calculate summed percentile intensity
            if (VibratorIntensities[i] > 0)
            {
                intTmp = VibratorIntensities[i];
            }
            if (VibratorMotionIntensities[i] > 0 && VibratorLifeSpans[i] > 0)
            {
                intTmp += VibratorMotionIntensities[i];
            }
            if (VibratorAdditionalIntensities[i] > 0 && VibratorAddiLifeSpans[i] > 0)
            {
                intTmp += VibratorAdditionalIntensities[i];
            }

            // cut with 100%, multiplied by weighting, then cut by maximum intensity allowed.
            intTmp = Mathf.Min((Mathf.Min(intTmp, 100) * VibratorIntensityWeight[i] / 100), maxValue);

            WifiToArduinoBytes[i] = System.Convert.ToByte((char)intTmp);

            VibratorLifeSpans[i] -= FramesPerUpdate * Time.fixedDeltaTime;
            VibratorAddiLifeSpans[i] -= FramesPerUpdate * Time.fixedDeltaTime;
        }
        if (!Arduino.arduinoPaused)
        {
            Arduino.writeToArduinoByte(WifiToArduinoBytes);
        }
        // yield return new WaitForSeconds(updateInterval);
        for(int i = 0; i < FramesPerUpdate; i++)
        {
            yield return new WaitForFixedUpdate();

        }
        //yield return new WaitForFixedUpdate();
        // yield return 0;
        currentMonitor = StartCoroutine(sendHeadbandStateToArduino());
        yield break;
    }

    
}
