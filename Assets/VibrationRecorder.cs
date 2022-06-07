using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AirDriVR;

public class VibrationRecorder : MonoBehaviour
{
    public string FilePath = @"D:\vibrateRecords\";
    //public string FileName = "0.csv";
    public float recordLength = 10.0f;
    private bool isRecording = false;
    public int recordCount = 0;
    public byte[] vibrationData = new byte[16];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 16; i++)
        {
            vibrationData[i] = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRecording && Input.GetKeyDown(KeyCode.R))
        {
            isRecording = true;
            StartCoroutine(StartRecording());
        }
    }

    private IEnumerator StartRecording()
    {
        Debug.Log("Start recording parameters!");

        string fileName = FilePath + "Vibration For Calibration_" + recordCount.ToString() + ".csv";
        StreamWriter writer = new StreamWriter(fileName);

        recordCount++;

        float timeStamp = 0.0f;
        while (timeStamp < recordLength)
        {
            WriteWithCSVFormat(writer);
            timeStamp += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        isRecording = false;
        Debug.Log("Record finished!");
        writer.Close();
        yield break;
    }

    private void WriteWithCSVFormat(StreamWriter writer)
    {
        string line = vibrationData[0].ToString() + "," + vibrationData[1].ToString() + "," + vibrationData[2].ToString() + "," + vibrationData[3].ToString() + ","
            + vibrationData[4].ToString() + "," + vibrationData[5].ToString() + "," + vibrationData[6].ToString() + "," + vibrationData[7].ToString() + ","
            + vibrationData[8].ToString() + "," + vibrationData[9].ToString() + "," + vibrationData[10].ToString() + "," + vibrationData[11].ToString() + ","
            + vibrationData[12].ToString() + "," + vibrationData[13].ToString() + "," + vibrationData[14].ToString() + "," + vibrationData[15].ToString();
        writer.WriteLine(line);
    }
}
