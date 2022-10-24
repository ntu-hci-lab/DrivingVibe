using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class WifiEncoder : Encoder
{
    private WifiToArduino Arduino;
    private bool isMonitorOn = false;
    private byte[] WifiToArduinoBytes = new byte[16];
    private Coroutine currentMonitor = null;

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
        float tmp = 0;
        int motorValue = 0;
        int IntervalSize = maxValue - minValue;
        float sum = 0;
        for (int i = 0; i < 16; i++)
        {
            if(virtualHeadband.HeadbandIntensityAfterOffset[i] > 0)
            {
                // start from 0~100%, multiplied by weighting and global multiplier
                tmp = (virtualHeadband.HeadbandIntensityAfterOffset[i] / 100.0f) * (globalMultiplier / 100.0f) * (VibratorIntensityWeight[i] / 100.0f);

                // sum for static version
                sum += tmp;

                // map 0~100% to 8~40, i.e. 8 + X% * 32, taking ceiling
                motorValue = Mathf.CeilToInt(minValue + tmp * IntervalSize);
            }
            else
            {
                // sum += 0;
                motorValue = 0;
            }

            WifiToArduinoBytes[i] = System.Convert.ToByte((char)motorValue);
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
