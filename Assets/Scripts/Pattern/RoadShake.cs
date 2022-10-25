using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class RoadShake : Pattern
{
    public float TyreSpeedMinThreshold = 0.02f;
    public float TyreSpeedMaxThreshold = 0.4f;
    [HideInInspector]
    public float[] SuspensionDiff = new float[4];

    // front right: 0 1 2 3 4
    private int[] frontRightIndice = { 1, 2, 3 };
    // front left: 12 13 14 15 0
    private int[] frontLeftIndice = { 13, 14, 15 };
    // back right: 4 5 6 7 8
    private int[] backRightIndice = { 5, 6, 7 };
    // back left: 8 9 10 11 12
    private int[] backLeftIndice = { 9, 10, 11 };

    private void FixedUpdate()
    {
        if (!IsEnable)
        {
            SetIntensitiesToZero();
            return;
        }

        SuspensionDiff = gameObject.GetComponent<ACListener>().suspensionDiff;
        //TyreSpeedMinThreshold = 0.005f + Mathf.Clamp(speed / 15.0f ,0.0f,15.0f) * 0.025f;
        //TyreSpeedMaxThreshold = 0.05f + Mathf.Clamp(speed / 15.0f, 0.0f, 15.0f) * 0.25f;
        for (int i = 0; i < 3; i++)
        {
            // front left: 12 13 14 15 0
            if (SuspensionDiff[0] > TyreSpeedMinThreshold)
            {
                HeadbandIntensities[frontLeftIndice[i]] = Mathf.FloorToInt(calculateIntensity(SuspensionDiff[0]));
            }
            else
            {
                HeadbandIntensities[frontLeftIndice[i]] = 0;
            }

            // front right: 0 1 2 3 4
            if (SuspensionDiff[1] > TyreSpeedMinThreshold)
            {
                HeadbandIntensities[frontRightIndice[i]] = Mathf.FloorToInt(calculateIntensity(SuspensionDiff[1]));
            }
            else
            {
                HeadbandIntensities[frontRightIndice[i]] = 0;
            }

            // back left: 8 9 10 11 12
            if (SuspensionDiff[2] > TyreSpeedMinThreshold)
            {
                HeadbandIntensities[backLeftIndice[i]] = Mathf.FloorToInt(calculateIntensity(SuspensionDiff[2]));
            }
            else
            {
                HeadbandIntensities[backLeftIndice[i]] = 0;
            }

            // back right: 4 5 6 7 8
            if (SuspensionDiff[3] > TyreSpeedMinThreshold)
            {
                HeadbandIntensities[backRightIndice[i]] = Mathf.FloorToInt(calculateIntensity(SuspensionDiff[3]));
            }
            else
            {
                HeadbandIntensities[backRightIndice[i]] = 0;
            }
        }
    }

    private float calculateIntensity(float mag)
    {
        float ret = 0;
        // Cropping
        mag = Mathf.Min(TyreSpeedMaxThreshold, mag);
        mag = Mathf.Max(mag, TyreSpeedMinThreshold);
        ret = (mag - TyreSpeedMinThreshold) / (TyreSpeedMaxThreshold - TyreSpeedMinThreshold) * 100.0f;
        return ret;
    }

}
