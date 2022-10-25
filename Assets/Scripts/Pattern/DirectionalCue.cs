using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class DirectionalCue : Pattern
{
    public float AngleThreshold = 45.0f;
    public float GforceMinThreshold = 0.1f;
    public float GforceMaxThreshold = 1.3f;

    [Header("Number Inspection")]
    //[HideInInspector]
    public Vector2 PlanarGforce = Vector2.zero;
    [HideInInspector]
    public Vector3 Gforce = Vector3.zero;
    //[HideInInspector]
    public float PlanarGforceMagnitude;
    [HideInInspector]
    public float GforceAngle;

    private void FixedUpdate()
    {
        if (!IsEnable)
        {
            SetIntensitiesToZero();
            return;
        }

        PlanarGforce = -1 * GetComponent<ACListener>().Gforce;
        PlanarGforceMagnitude = PlanarGforce.magnitude;
        GforceAngle = Vector2.SignedAngle(PlanarGforce, new Vector2(0.0f, 1.0f));

        if (PlanarGforceMagnitude > GforceMinThreshold)
        {
            convertGforce(GforceAngle, PlanarGforceMagnitude);
        }
        else
        {
            SetIntensitiesToZero();
        }
    }
    private void convertGforce(float angle, float magnitude)
    {
        float angleDiff = 0;
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(angle - AngleOfEachVibrator[i]) < AngleThreshold)
            {
                angleDiff = Mathf.Abs(angle - AngleOfEachVibrator[i]);
                HeadbandIntensities[i] = Mathf.FloorToInt(calculateIntensity(angleDiff, magnitude));
            }
            else if (Mathf.Abs(angle - AngleOfEachVibrator[i] + 360.0f) < AngleThreshold)
            {
                angleDiff = Mathf.Abs(angle - AngleOfEachVibrator[i] + 360.0f);
                HeadbandIntensities[i] = Mathf.FloorToInt(calculateIntensity(angleDiff, magnitude));
            }
            else if (Mathf.Abs(angle - AngleOfEachVibrator[i] - 360.0f) < AngleThreshold)
            {
                angleDiff = Mathf.Abs(angle - AngleOfEachVibrator[i] - 360.0f);
                HeadbandIntensities[i] = Mathf.FloorToInt(calculateIntensity(angleDiff, magnitude));
            }
            else
            {
                HeadbandIntensities[i] = 0;
            }
        }
    }

    private float calculateIntensity(float angleDiff, float G_mag)
    {
        float ret = 0;
        float angleMult = (AngleThreshold - angleDiff) / AngleThreshold;
        // Cropping
        G_mag = Mathf.Min(GforceMaxThreshold, G_mag);
        G_mag = Mathf.Max(G_mag, GforceMinThreshold);
        ret = (G_mag - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold) * angleMult * 100.0f;
        return ret;
    }

}
