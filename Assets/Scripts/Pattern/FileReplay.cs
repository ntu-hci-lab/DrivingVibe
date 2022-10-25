using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AirDriVR;

public class FileReplay : Pattern
{
    public bool isSync = false;
    public bool isMirroring = false;
    public string profileFilePath = @"D:\vibrateRecords\";
    public string profileFileName = "Profile_minute0.csv";
    private StreamReader profileReader;
    private ACListener listener;
    // For sync
    private float _gas;
    // Start is called before the first frame update
    void Start()
    {
        listener = GetComponent<ACListener>();
        // Read headband intensity from files
        string path = Path.Combine(profileFilePath, profileFileName);
        FileInfo patternPreviewFile = new FileInfo(path);
        profileReader = patternPreviewFile.OpenText();
        StartCoroutine(UpdateHeadbandFromFile());
    }
    private IEnumerator UpdateHeadbandFromFile()
    {
        // skip the first line
        string line = profileReader.ReadLine();
        do
        {
            line = profileReader.ReadLine();
            parseLineToHeadband(line);
        } while (_gas <= 0);
        // _gas > 0, ready to update
        while (listener.gas <= 0)
        {
            // wait until in-game gas is also on
            yield return new WaitForFixedUpdate();
        }
        isSync = true;
        while (line != null)
        {
            line = profileReader.ReadLine();
            parseLineToHeadband(line);
            yield return new WaitForFixedUpdate();
        }
        profileReader.Close();
    }

    private void parseLineToHeadband(string line)
    {
        string[] VibrationRecords = line.Split(',');
        // VibrationRecords[7]: gas
        _gas = float.Parse(VibrationRecords[7]);
        if (isMirroring)
        {
            // VibrationRecords[13]: left motor
            // VibrationRecords[14]: right motor
            for (int i = 0; i < 16; i++)
            {
                int left = int.Parse(VibrationRecords[13]);
                int right = int.Parse(VibrationRecords[14]);
                float percentageIntensity = (float)(left + right) / 510.0f;
                HeadbandIntensities[i] = Mathf.CeilToInt(percentageIntensity * 100);
            }
        }
        else
        {
            // VibrationRecords[47~62]: sum intensity
            for (int i = 0; i < 16; i++)
            {
                HeadbandIntensities[i] = int.Parse(VibrationRecords[i + 47]);
            }
        }
    }
}
