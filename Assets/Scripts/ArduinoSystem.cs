using System;
using UnityEngine;
using System.IO.Ports;
using System.IO;
using System.Collections;

public class ArduinoSystem : MonoBehaviour
{
    private byte[] setZero = new byte[3];

    public string serialName = "COM12";
    public int baudRate = 115200;
    public SerialPort sp;

    public bool arduinoPaused;

    public bool showString = true;


    // Start is called before the first frame update
    void Start()
    {
        setZero[0] = System.Convert.ToByte((char)0);
        setZero[1] = System.Convert.ToByte((char)100);
        setZero[2] = System.Convert.ToByte((char)0);

        // arduinoPaused = false;
        // showString = true;

        sp = new SerialPort(serialName, baudRate);
        sp.Open();
    }

    public void startArduino()
    {
        arduinoPaused = false;
    }

    public void PauseArduino()
    {
        arduinoPaused = true;
        setAllToZero();
    }

    public void writeToArduinoByte(byte[] input)
    {
        if (showString && !arduinoPaused)
        {
            Debug.Log("Send to arduino: " + input[0].ToString() + " " + input[1].ToString() + " " + input[2].ToString());
        }
        if (sp != null && sp.IsOpen && !arduinoPaused)
        {
            sp.Write(input, 0, 3);
        }
    }

    public void ReOpen()
    {
        sp = new SerialPort(serialName, baudRate);
        sp.Open();
    }

    public void setAllToZero()
    {
        Debug.Log("Set all to zero");
        for(int i = 0; i < 12; i++)
        {
            setZero[0] = System.Convert.ToByte((char)i);
            if (sp != null && sp.IsOpen)
            {
                sp.Write(setZero, 0, 3);
            }
        }
    }

    public void sendTest()
    {
        if (sp != null && sp.IsOpen)
        {
            sp.Write(setZero, 0, 3);
        }
    }


    private void OnApplicationQuit()
    {
        setAllToZero();
        sp.Close();
    }

}
