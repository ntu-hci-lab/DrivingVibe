using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pattern : MonoBehaviour
{
    // Refactor version of pattern generator

    public bool IsEnable = true;
    public float[] AngleOfEachVibrator = new float[16]; // 0 degree -> forward, increase clockwisely from -180 ~ 180;
    public int[] HeadbandIntensities = new int[16];

    public Pattern()
    {
        AngleOfEachVibrator[0] = 0.0f;
        AngleOfEachVibrator[1] = -22.5f;
        AngleOfEachVibrator[2] = -45.0f;
        AngleOfEachVibrator[3] = -67.5f;
        AngleOfEachVibrator[4] = -90.0f;
        AngleOfEachVibrator[5] = -112.5f;
        AngleOfEachVibrator[6] = -135.0f;
        AngleOfEachVibrator[7] = -157.5f;
        AngleOfEachVibrator[8] = 180.0f;
        AngleOfEachVibrator[9] = 157.5f;
        AngleOfEachVibrator[10] = 135.0f;
        AngleOfEachVibrator[11] = 112.5f;
        AngleOfEachVibrator[12] = 90.0f;
        AngleOfEachVibrator[13] = 67.5f;
        AngleOfEachVibrator[14] = 45.0f;
        AngleOfEachVibrator[15] = 22.5f;

        SetIntensitiesToZero();
    }

    public void SetIntensitiesToZero()
    {
        for (int i = 0; i < 16; i++)
        {
            HeadbandIntensities[i] = 0;
        }
    }
}
