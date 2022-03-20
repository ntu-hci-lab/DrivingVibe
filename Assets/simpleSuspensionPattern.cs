using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class simpleSuspensionPattern : MonoBehaviour
{
    public bool lowSusVibration;

    public bool frequencyMappingEnable;

    public float[] suspension = new float[4];

    private float speed;
    public float speedMinThreshold = 0.0f;
    public float speedMaxThreshold = 30.0f;

    public float suspensionThreshold;
    private float frontBaseSuspension = 0.076f;
    private float backBaseSuspension = 0.071f;

    // front right: 0 1 2 3 4
    private  int[] frontRightIndice = { 0, 1, 2, 3, 4};
    // front left: 12 13 14 15 0
    private int[] frontLeftIndice = { 12, 13, 14, 15, 0 };
    // back right: 4 5 6 7 8
    private int[] backRightIndice = { 4, 5, 6, 7, 8 };
    // back left: 8 9 10 11 12
    private int[] backLeftIndice = { 8, 9, 10, 11, 12 };

    private VirtualHeadband virtualHeadband;

    private void Start()
    {
        virtualHeadband = gameObject.GetComponent<VirtualHeadband>();
    }

    // Update is called once per frame
    void Update()
    {
        speed = GetComponent<ACListener>().velocity;

        float suspensionOffset;


        if (lowSusVibration)
        {
            for (int i = 0; i < 5; i++)
            {
                suspensionOffset = backBaseSuspension - suspension[0];
                if (suspensionOffset > suspensionThreshold)
                {
                    // back right: 4 5 6 7 8
                    virtualHeadband.VibratorIntensities[backRightIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[backRightIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[backRightIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[backRightIndice[i]] = 0.0f;
                }

                suspensionOffset = backBaseSuspension - suspension[1];
                if (suspensionOffset > suspensionThreshold)
                {
                    // back left: 8 9 10 11 12
                    virtualHeadband.VibratorIntensities[backLeftIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[backLeftIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[backLeftIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[backLeftIndice[i]] = 0.0f;
                }

                suspensionOffset = frontBaseSuspension - suspension[2];
                if (suspensionOffset > suspensionThreshold)
                {
                    // front right: 0 1 2 3 4
                    virtualHeadband.VibratorIntensities[frontRightIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[frontRightIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[frontRightIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[frontRightIndice[i]] = 0.0f;
                }

                suspensionOffset = frontBaseSuspension - suspension[3];
                if (suspensionOffset > suspensionThreshold)
                {
                    // front left: 12 13 14 15 0
                    virtualHeadband.VibratorIntensities[frontLeftIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[frontLeftIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[frontLeftIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[frontLeftIndice[i]] = 0.0f;
                }
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                suspensionOffset = suspension[0] - backBaseSuspension;
                if (suspensionOffset > suspensionThreshold)
                {
                    // back right: 4 5 6 7 8
                    virtualHeadband.VibratorIntensities[backRightIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[backRightIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[backRightIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[backRightIndice[i]] = 0.0f;
                }

                suspensionOffset = suspension[1] - backBaseSuspension;
                if (suspensionOffset > suspensionThreshold)
                {
                    // back left: 8 9 10 11 12
                    virtualHeadband.VibratorIntensities[backLeftIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[backLeftIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[backLeftIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[backLeftIndice[i]] = 0.0f;
                }

                suspensionOffset = suspension[2] - frontBaseSuspension;
                if (suspensionOffset > suspensionThreshold)
                {
                    // front right: 0 1 2 3 4
                    virtualHeadband.VibratorIntensities[frontRightIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[frontRightIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[frontRightIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[frontRightIndice[i]] = 0.0f;
                }

                suspensionOffset = suspension[3] - frontBaseSuspension;
                if (suspensionOffset > suspensionThreshold)
                {
                    // front left: 12 13 14 15 0
                    virtualHeadband.VibratorIntensities[frontLeftIndice[i]] = calculateIntensity(suspensionOffset);
                    virtualHeadband.VibratorLifeSpans[frontLeftIndice[i]] = 1.0f;
                }
                else
                {
                    virtualHeadband.VibratorIntensities[frontLeftIndice[i]] = 0;
                    virtualHeadband.VibratorLifeSpans[frontLeftIndice[i]] = 0.0f;
                }
            }
        }
    }

    private int calculateIntensity(float offset)
    {
        // max acceptable offset is 0.04f
        offset = Mathf.Min(offset, 0.04f);
        float intensity = (offset - suspensionThreshold) / (0.04f - suspensionThreshold) * 100.0f;
        int ret = Mathf.FloorToInt(intensity);
        return ret;
    }

}
