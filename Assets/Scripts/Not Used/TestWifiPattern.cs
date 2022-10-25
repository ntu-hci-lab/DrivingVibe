using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWifiPattern : MonoBehaviour
{
    private WifiToArduino wifi;
    private int[] VibratorIntensities = new int[16];
    public float interval = 1.0f;
    public int intensity = 20;
    public enum PatternChoose
    {
        Cycle = 0,
        Static = 1,
        AllOnOff = 2
    }
    public PatternChoose patternChoose = PatternChoose.AllOnOff;

    void Start()
    {
        wifi = gameObject.GetComponent<WifiToArduino>();
        //virtualLayer = gameObject.GetComponent<VirtualLayer>();
        //VibratorIntensities = virtualLayer.VibratorIntensities;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (patternChoose)
            {
                case PatternChoose.Cycle:
                    StartCoroutine(CyclePattern());
                    break;
                case PatternChoose.Static:
                    StartCoroutine(StaticPattern());
                    break;
                case PatternChoose.AllOnOff:
                    AllOnOffPattern();
                    break;
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StopAllCoroutines();
            //wifi.setAllToZero();
            //virtualLayer.setAllToZero();
        }
    }


    private IEnumerator CyclePattern()
    {
        for(int i = 0; i < 16; i++)
        {
            for(int j = 0; j < 16; j++)
            {
                VibratorIntensities[j] = 0;
            }
            VibratorIntensities[i] = intensity;
            yield return new WaitForSeconds(interval);
        }
        yield break;
    }
    private IEnumerator StaticPattern()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                VibratorIntensities[j] = intensity;
            }
            yield return new WaitForSeconds(interval);
            for (int j = 0; j < 16; j++)
            {
                VibratorIntensities[j] = 0;
            }
            yield return new WaitForSeconds(interval);
        }
        yield break;
    }

    private void AllOnOffPattern()
    {
        byte[] input = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            VibratorIntensities[i] = intensity - VibratorIntensities[i];
            input[i] = System.Convert.ToByte((char)VibratorIntensities[i]);
        }
        // wifi.writeToArduinoByte(input);
    }
}
