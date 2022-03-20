using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AirDriVR;

public class ParameterRecorder : MonoBehaviour
{
    public string FilePath = @"D:\vibrateRecords\";
    //public string FileName = "0.csv";
    public float recordLength = 10.0f;
    private bool isRecording = false;
    private float[] data = new float[7];
    public int recordCount = 0;
    private ACListener listener;


    public float[] suspensionDiff = new float[4];
    // Start is called before the first frame update
    void Start()
    {
        //recordCount = 0;
        for(int i = 0; i < 7; i++)
        {
            data[0] = 0.0f;
        }
        listener = GetComponent<ACListener>();
        suspensionDiff = listener.suspensionDiff;
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

        string SusFileName = FilePath + "Suspension" + recordCount.ToString() + ".csv";
        StreamWriter SusDiffWriter = new StreamWriter(SusFileName);
        string AccFileName = FilePath + "Acc" + recordCount.ToString() + ".csv";
        StreamWriter AccDiffWriter = new StreamWriter(AccFileName);
        string VelocityFileName = FilePath + "Velocity" + recordCount.ToString() + ".csv";
        StreamWriter VelocityWriter = new StreamWriter(VelocityFileName);

        recordCount++;

        float timeStamp = 0.0f;
        while (timeStamp < recordLength)
        {
            GetParameter();
            WriteWithCSVFormat(SusDiffWriter, AccDiffWriter, VelocityWriter);
            timeStamp += Time.deltaTime;
            yield return 0;
        }
        isRecording = false;
        Debug.Log("Record finished!");
        SusDiffWriter.Close();
        AccDiffWriter.Close();
        VelocityWriter.Close();
        yield break;
    }

    private void GetParameter()
    {
        data[0] = suspensionDiff[0];
        data[1] = suspensionDiff[1];
        data[2] = suspensionDiff[2];
        data[3] = suspensionDiff[3];
        data[4] = listener.Gforce.x;
        data[5] = listener.Gforce.y;
        data[6] = listener.velocity;
    }
    private void WriteWithCSVFormat(StreamWriter SusDiffWriter, StreamWriter AccDiffWriter, StreamWriter VelocityWriter)
    {
        string line = Time.time.ToString() + "," + data[0].ToString() + "," + data[1].ToString() + "," + data[2].ToString() + "," + data[3].ToString();
        SusDiffWriter.WriteLine(line); 
        line = Time.time.ToString() + "," + data[4].ToString() + "," + data[5].ToString();
        AccDiffWriter.WriteLine(line);
        line = Time.time.ToString() + "," + data[6].ToString();
        VelocityWriter.WriteLine(line);
    }
}
