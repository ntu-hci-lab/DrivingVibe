using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.IO;
using UnityEngine;


public class PingTest : MonoBehaviour
{
    private byte[] data = new byte[3];
    private WifiToArduino wifi;
    public string serialName = "COM3";
    public int baudRate = 115200;
    public SerialPort sp;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 3; i++)
        {
            data[i] = System.Convert.ToByte((char)i);
        }
        wifi = GetComponent<WifiToArduino>();
        sp = new SerialPort(serialName, baudRate);
        sp.Open();
        if (sp.IsOpen)
        {
            Debug.Log("Serial Ready");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            wifi.writeToArduinoByte(data);
            Debug.Log("Wifi sent!");
            sp.Write(data, 0, 3);
            Debug.Log("Serial sent!");
            string receiveString = sp.ReadLine();
            Debug.Log("Ping Received: " + receiveString + " microseconds.");
            /*
            byte[] receiveData = new byte[5];
            sp.Read(receiveData, 0, 4);
            int ping = System.BitConverter.ToInt32(receiveData, 0);
            Debug.Log(receiveData[0].ToString() + " " + receiveData[1].ToString() + " " + receiveData[2].ToString() + " " + receiveData[3].ToString());
            */
        }
    }
}
