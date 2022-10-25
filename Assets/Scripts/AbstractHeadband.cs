using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class AbstractHeadband : MonoBehaviour
{
    // Refactored version of virtual headband

    public bool isControllerHaptic = false;
    // From 0~100 intensity
    public int[] HeadbandIntensity = new int[16];
    public int[] HeadbandIntensityAfterOffset = new int[16];
    public List<Pattern> Patterns;
    public int MotorOffset;

    private float HeadRotOffset;
    private float FrontAngle;

    private void Start()
    {
        if (isControllerHaptic)
        {
            Patterns.Add(GetComponent<Mirroring>());
        }
        else
        {
            Patterns.Add(GetComponent<DirectionalCue>());
            Patterns.Add(GetComponent<RoadShake>());
            Patterns.Add(GetComponent<ColdStart>());
        }

        for (int i = 0; i < 16; i++)
        {
            HeadbandIntensity[i] = 0;
            HeadbandIntensityAfterOffset[i] = 0;
        }
        StartCoroutine(setInitialHeadRot());
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < 16; i++)
        {
            HeadbandIntensity[i] = 0;
        }
        foreach (Pattern pattern in Patterns)
        {
            for (int i = 0; i < 16; i++)
            {
                HeadbandIntensity[i] += pattern.HeadbandIntensities[i];
            }
        }
        for (int i = 0; i < 16; i++)
        {
            HeadbandIntensity[i] = Mathf.Max(0, Mathf.Min(HeadbandIntensity[i], 100));
        }


        HeadRotOffset = FrontAngle - GetComponent<BackgroundVRListener>().HeadRotation.eulerAngles.y;
        MotorOffset = (Mathf.RoundToInt(HeadRotOffset / 22.5f) + 16) % 16;
        for (int i = 0; i < 16; i++)
        {
            HeadbandIntensityAfterOffset[(i + MotorOffset) % 16] = HeadbandIntensity[i];
        }
    }

    private IEnumerator setInitialHeadRot()
    {
        while (GetComponent<BackgroundVRListener>().HeadRotation.eulerAngles.y == 0)
        {
            yield return 0;
        }
        FrontAngle = GetComponent<BackgroundVRListener>().HeadRotation.eulerAngles.y;
    }
}
