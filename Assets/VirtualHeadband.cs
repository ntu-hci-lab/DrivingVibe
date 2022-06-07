using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualHeadband : MonoBehaviour
{
    // From 0~100 intensity
    public int[] HeadbandIntensity = new int[16];

    // From 0~100 intensity
    public int[] DirectionalCueIntensities = new int[16];
    // From 0~100 intensity
    public int[] TactileMotionIntensities = new int[16];
    // From 0~100 intensity
    public int[] RoadShakeIntensities = new int[16];
    // For tactile motion
    public float[] TactileMotionLifeSpans = new float[16];

    private PatternGenerator patternGenerator;
    private void Start()
    {
        patternGenerator = GetComponent<PatternGenerator>();
        DirectionalCueIntensities = patternGenerator.DirectionalCueIntensities;
        TactileMotionIntensities = patternGenerator.TactileMotionIntensities;
        RoadShakeIntensities = patternGenerator.DirectionalCueIntensities;
        TactileMotionLifeSpans = patternGenerator.TactileMotionLifeSpans;
    }
    private void Update()
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
    }
}
