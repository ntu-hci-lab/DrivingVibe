using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AirDriVR;

public class VirtualHeadband : VirtualLayer
{
    private ArduinoSystem Arduino;
    private bool isMonitorOn = false;
    private byte[] ToArduinoBytes = new byte[3];
    private Coroutine currentMonitor;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        Arduino = gameObject.GetComponent<ArduinoSystem>();
        if (!Arduino.arduinoPaused)
        {
            // arduino on, start sending
            currentMonitor = StartCoroutine(sendHeadbandStateToArduino());
            isMonitorOn = true;
        }
    }
    private void Update()
    {
        if (Arduino.arduinoPaused)
        {
            // arduino off, stop sending
            StopAllCoroutines();
            isMonitorOn = false;
        }
        if (!isMonitorOn && !Arduino.arduinoPaused)
        {
            // arduino on, start sending
            currentMonitor = StartCoroutine(sendHeadbandStateToArduino());
            isMonitorOn = true;
        }
    }
    private IEnumerator sendHeadbandStateToArduino()
    {
        int intTmp = 0;
        for (int i = 0; i < 16; i++)
        {            
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

            ToArduinoBytes[0] = System.Convert.ToByte((char)i);
            ToArduinoBytes[1] = System.Convert.ToByte((char)(170 / 2));
            ToArduinoBytes[2] = System.Convert.ToByte((char)intTmp);
            if (!Arduino.arduinoPaused)
            {
                Arduino.writeToArduinoByte(ToArduinoBytes);
            }
            VibratorLifeSpans[i] -= FramesPerUpdate * Time.fixedDeltaTime;
            VibratorAddiLifeSpans[i] -= FramesPerUpdate * Time.fixedDeltaTime;
        }

        //yield return new WaitForSeconds(updateInterval);
        for (int i = 0; i < FramesPerUpdate; i++)
        {
            yield return new WaitForFixedUpdate();

        }
        // yield return 0;
        currentMonitor = StartCoroutine(sendHeadbandStateToArduino());
        yield break;
    }
}
