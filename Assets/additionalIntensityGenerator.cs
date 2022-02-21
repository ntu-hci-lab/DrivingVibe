using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AirDriVR;

public class additionalIntensityGenerator : MonoBehaviour
{
    public bool feedbackEnable;

    public float maxAddiIntensity = 50.0f;
    public float minThreshold;
    public float maxThreshold;
    public float duration;

    public float diffAmplifyFactor = 10.0f;
    private float[] lastSuspension = new float[4];
    private float[] newSuspension = new float[4];
    public float[] suspensionDiff = new float[4];

    // front right: 0 1 2 3 4
    private int[] frontRightIndice = { 1, 2, 3 };
    // front left: 12 13 14 15 0
    private int[] frontLeftIndice = { 13, 14, 15 };
    // back right: 4 5 6 7 8
    private int[] backRightIndice = { 5, 6, 7 };
    // back left: 8 9 10 11 12
    private int[] backLeftIndice = { 9, 10, 11 };

    private VirtualHeadband virtualHeadband;


    void Start()
    {
        virtualHeadband = gameObject.GetComponent<VirtualHeadband>();
        StartCoroutine(waitThenStartSuspensionMonitor());
    }

    private IEnumerator waitThenStartSuspensionMonitor()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < 4; i++)
        {
            newSuspension[i] = gameObject.GetComponent<ACListener>().suspension[i];
            suspensionDiff[i] = Mathf.Abs(newSuspension[i] - lastSuspension[i]) * diffAmplifyFactor;
            lastSuspension[i] = newSuspension[i];
        }
        StartCoroutine(suspensionMonitor());
    }


    // Update is called once per frame
    private IEnumerator suspensionMonitor()
    {
        for (int i = 0; i < 4; i++)
        {
            newSuspension[i] = gameObject.GetComponent<ACListener>().suspension[i];
            suspensionDiff[i] = Mathf.Abs(newSuspension[i] - lastSuspension[i]) * diffAmplifyFactor;
            lastSuspension[i] = newSuspension[i];
        }
        if (feedbackEnable)
        {
            for (int i = 0; i < 3; i++)
            {
                if (suspensionDiff[0] > minThreshold)
                {
                    // back right: 4 5 6 7 8
                    virtualHeadband.VibratorAdditionalIntensities[backRightIndice[i]] = calculateIntensity(suspensionDiff[0]);
                    virtualHeadband.VibratorAddiLifeSpans[backRightIndice[i]] = duration;
                }
                else
                {
                    virtualHeadband.VibratorAdditionalIntensities[backRightIndice[i]] = 0;
                }

                if (suspensionDiff[1] > minThreshold)
                {
                    // back left: 8 9 10 11 12
                    virtualHeadband.VibratorAdditionalIntensities[backLeftIndice[i]] = calculateIntensity(suspensionDiff[1]);
                    virtualHeadband.VibratorAddiLifeSpans[backLeftIndice[i]] = duration;
                }
                else
                {
                    virtualHeadband.VibratorAdditionalIntensities[backLeftIndice[i]] = 0;
                }

                if (suspensionDiff[2] > minThreshold)
                {
                    // front right: 0 1 2 3 4
                    virtualHeadband.VibratorAdditionalIntensities[frontRightIndice[i]] = calculateIntensity(suspensionDiff[2]);
                    virtualHeadband.VibratorAddiLifeSpans[frontRightIndice[i]] = duration;
                }
                else
                {
                    virtualHeadband.VibratorAdditionalIntensities[frontRightIndice[i]] = 0;
                }

                if (suspensionDiff[3] > minThreshold)
                {
                    // front left: 12 13 14 15 0
                    virtualHeadband.VibratorAdditionalIntensities[frontLeftIndice[i]] = calculateIntensity(suspensionDiff[3]);
                    virtualHeadband.VibratorAddiLifeSpans[frontLeftIndice[i]] = duration;
                }
                else
                {
                    virtualHeadband.VibratorAdditionalIntensities[frontLeftIndice[i]] = 0;
                }
            }
        }

        yield return new WaitForSeconds(duration);
        StartCoroutine(suspensionMonitor());
        yield break;
    }

    private int calculateIntensity(float diff)
    {
        diff = Mathf.Min(diff, maxThreshold);
        float intensity = (diff - minThreshold) / (maxThreshold - minThreshold) * maxAddiIntensity;
        int ret = Mathf.FloorToInt(intensity);
        return ret;
    }
}
