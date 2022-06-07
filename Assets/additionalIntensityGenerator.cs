using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AirDriVR;

public class additionalIntensityGenerator : MonoBehaviour
{
    public bool feedbackEnable;

    //public float maxAddiIntensity = 50.0f;
    public float minThreshold;
    public float maxThreshold;
    public float duration;

    public float diffAmplifyFactor = 10.0f;
    public float[] suspensionDiff = new float[4];

    // front right: 0 1 2 3 4
    private int[] frontRightIndice = { 1, 2, 3 };
    // front left: 12 13 14 15 0
    private int[] frontLeftIndice = { 13, 14, 15 };
    // back right: 4 5 6 7 8
    private int[] backRightIndice = { 5, 6, 7 };
    // back left: 8 9 10 11 12
    private int[] backLeftIndice = { 9, 10, 11 };

    //private VirtualLayer virtualLayer;


    void Start()
    {
        //virtualLayer = gameObject.GetComponent<VirtualLayer>();
        StartCoroutine(suspensionMonitor());
    }
    // Update is called once per frame
    private IEnumerator suspensionMonitor()
    {
        //yield return new WaitForSeconds(0.5f);
        suspensionDiff = gameObject.GetComponent<ACListener>().suspensionDiff;
        if (feedbackEnable)
        {
            for (int i = 0; i < 4; i++)
            {
                if (suspensionDiff[0] > minThreshold)
                {
                    // back right: 4 5 6 7 8
                    //virtualLayer.VibratorAdditionalIntensities[backRightIndice[i]] = calculateIntensity(suspensionDiff[0]);
                }
                else
                {
                    //virtualLayer.VibratorAdditionalIntensities[backRightIndice[i]] = 0;
                }

                if (suspensionDiff[1] > minThreshold)
                {
                    // back left: 8 9 10 11 12
                    //virtualLayer.VibratorAdditionalIntensities[backLeftIndice[i]] = calculateIntensity(suspensionDiff[1]);
                }
                else
                {
                    //virtualLayer.VibratorAdditionalIntensities[backLeftIndice[i]] = 0;
                }

                if (suspensionDiff[2] > minThreshold)
                {
                    // front right: 0 1 2 3 4
                    //virtualLayer.VibratorAdditionalIntensities[frontRightIndice[i]] = calculateIntensity(suspensionDiff[2]);
                }
                else
                {
                    //virtualLayer.VibratorAdditionalIntensities[frontRightIndice[i]] = 0;
                }

                if (suspensionDiff[3] > minThreshold)
                {
                    // front left: 12 13 14 15 0
                    //virtualLayer.VibratorAdditionalIntensities[frontLeftIndice[i]] = calculateIntensity(suspensionDiff[3]);
                }
                else
                {
                    //virtualLayer.VibratorAdditionalIntensities[frontLeftIndice[i]] = 0;
                }
            }
        }

        //yield return new WaitForSeconds(duration);
        yield return 0;
        StartCoroutine(suspensionMonitor());
        yield break;
    }

    private int calculateIntensity(float diff)
    {
        diff = Mathf.Min(diff, maxThreshold);
        float intensity = diff / maxThreshold * 100.0f;
        int ret = Mathf.FloorToInt(intensity);
        return ret;
    }
}
