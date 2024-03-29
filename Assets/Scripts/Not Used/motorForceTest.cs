﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class motorForceTest : MonoBehaviour
{
    public WifiToArduino wifiToArduino;
    public byte[] data = new byte[16];
    public float interval = 0.5f;
    // Start is called before the first frame update
    async void Start()
    {
        for (int i = 0; i < 16; i++){
            data[i] = System.Convert.ToByte((char)0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)){
            StartCoroutine(autoMotor());
        }
    }

    private IEnumerator autoMotor()
    {
        for(int i = 5; i <= 200; i += 5){
            data[0] = System.Convert.ToByte((char)i);
            wifiToArduino.writeToArduinoByte(data);
            yield return new WaitForSeconds(interval);

            data[0] = System.Convert.ToByte((char)0);
            wifiToArduino.writeToArduinoByte(data);
            yield return new WaitForSeconds(interval);
        }
    }
}
