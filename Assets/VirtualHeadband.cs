using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AirDriVR;

public class VirtualHeadband : MonoBehaviour
{
    public bool isSync = false;
    // If true then update intensity from files
    public bool isPassive = false;
    public string profileFilePath = @"D:\vibrateRecords\";
    public string profileFileName = "Profile_minute0.csv";
    public bool isControllerHaptic = false;
    private StreamReader profileReader;
    // For sync
    public float _gas;

    // From 0~100 intensity
    public int[] HeadbandIntensity = new int[16];
    public int[] HeadbandIntensityAfterOffset = new int[16];

    // From 0~100 intensity
    public int[] DirectionalCueIntensities = new int[16];
    // From 0~100 intensity
    public int[] TactileMotionIntensities = new int[16];
    // From 0~100 intensity
    public int[] RoadShakeIntensities = new int[16];
    // For tactile motion
    public float[] TactileMotionLifeSpans = new float[16];

    private ACListener listener;
    private PatternGenerator patternGenerator;
    private ControllerHaptic controllerHaptic;

    private float HeadRotOffset;
    public int MotorOffset;
    private float FrontAngle;

    private void FixedUpdate()
    {
        HeadRotOffset = FrontAngle - GetComponent<BackgroundVRListener>().HeadRotation.eulerAngles.y;
        MotorOffset = (Mathf.RoundToInt(HeadRotOffset / 22.5f) + 16) % 16;
        for(int i = 0; i < 16; i++)
        {
            HeadbandIntensityAfterOffset[(i + MotorOffset) % 16] = HeadbandIntensity[i];
        }
    }

    private void Start()
    {
        for (int i = 0; i < 16; i++)
        {
            HeadbandIntensity[i] = 0;
            HeadbandIntensityAfterOffset[i] = 0;
        }
        StartCoroutine(setInitialHeadRot());
        if (isPassive)
        {
            listener = GetComponent<ACListener>();
            // Read headband intensity from files
            string path = Path.Combine(profileFilePath, profileFileName);
            FileInfo patternPreviewFile = new FileInfo(path);
            profileReader = patternPreviewFile.OpenText();
            StartCoroutine(UpdateHeadbandFromFile());
        }
        else
        {
            if (isControllerHaptic)
            {
                // Read headband intensity from ControllerHaptic
                controllerHaptic = GetComponent<ControllerHaptic>();
                HeadbandIntensity = controllerHaptic.headband;
                //StartCoroutine(UpdateHeadbandFromControllerHaptic());
            }
            else
            {
                // Read headband intensity from PatternGenerator
                patternGenerator = GetComponent<PatternGenerator>();
                DirectionalCueIntensities = patternGenerator.DirectionalCueIntensities;
                TactileMotionIntensities = patternGenerator.TactileMotionIntensities;
                RoadShakeIntensities = patternGenerator.RoadShakeIntensities;
                TactileMotionLifeSpans = patternGenerator.TactileMotionLifeSpans;
                StartCoroutine(UpdateHeadbandFromPatternGenerator());
            }
        }
    }

    private IEnumerator setInitialHeadRot()
    {
        while(GetComponent<BackgroundVRListener>().HeadRotation.eulerAngles.y == 0)
        {
            yield return 0;
        }
        FrontAngle = GetComponent<BackgroundVRListener>().HeadRotation.eulerAngles.y;
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
        while(listener.gas <= 0)
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
    private IEnumerator UpdateHeadbandFromPatternGenerator()
    {
        while (true)
        {
            int intTmp;
            for (int i = 0; i < 16; i++)
            {
                intTmp = DirectionalCueIntensities[i] + RoadShakeIntensities[i];

                if (TactileMotionLifeSpans[i] > 0)
                {
                    intTmp += TactileMotionIntensities[i];
                }
                // clamp with [0%, 100%]
                HeadbandIntensity[i] = Mathf.Max(0, Mathf.Min(intTmp, 100));
            }
            yield return 0;
        }
    }
    private void parseLineToHeadband(string line)
    {
        string[] VibrationRecords = line.Split(',');
        // VibrationRecords[7]: gas
        _gas = float.Parse(VibrationRecords[7]);
        if (isControllerHaptic)
        {
            // VibrationRecords[13]: left motor
            // VibrationRecords[14]: right motor
            for (int i = 0; i < 16; i++)
            {
                int left = int.Parse(VibrationRecords[13]);
                int right = int.Parse(VibrationRecords[14]);
                float percentageIntensity = (float)(left + right) / 510.0f;
                HeadbandIntensity[i] = Mathf.CeilToInt(percentageIntensity * 100);
            }
        }
        else
        {
            // VibrationRecords[47~62]: sum intensity
            for (int i = 0; i < 16; i++)
            {
                HeadbandIntensity[i] = int.Parse(VibrationRecords[i + 47]);
            }
        }
    }
}
