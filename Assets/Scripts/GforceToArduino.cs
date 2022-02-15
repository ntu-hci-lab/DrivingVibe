using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class GforceToArduino : MonoBehaviour
{

    //
    private ArduinoSystem arduinoSystem;

    public bool inertiaFeedbackEnable;
    private bool inertiaing;

    public int vibratorFrequency = 150;

    public int minIntensity = 30; // map to 0.15G & __ m/s^3
    public int maxIntensity = 200; // map to 1.0G & __ m/s^3

    public float GforceMinThreshold = 0.15f;
    public float GforceMaxThreshold = 1.0f;

    private int[] lastVibratorIntensities = new int[16];
    private int[] newVibratorIntensities = new int[16];
    public float angleThreshold = 30.0f; // bad method

    public float intensityUpdateThreshold = 3.0f;

    private int updateAmount = 0;
    private string toArduino;
    //
    public Vector3 Gforce;
    public Vector2 planarGforce;
    public float planarGforceMagnitude;
    public float GforceAngle;

    public float[] angleOfEachVibrator = new float[16]; // 0 degree -> forward, increase clockwisely from -180 ~ 180

    public bool GforceTrigger = false;
    private bool started = false;

    private int tmp;
    private int lastTmp;

    private void Awake()
    {
        checkAngles();
        inertiaing = false;
        Gforce = Vector3.zero;
        inertiaFeedbackEnable = true;
    }

    private void Start()
    {
        arduinoSystem = gameObject.GetComponent<ArduinoSystem>();
        for(int i = 0; i < 16; i++)
        {
            lastVibratorIntensities[i] = 0;
            newVibratorIntensities[i] = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        inertiaing = false;
        toArduino = null;
        updateAmount = 0;
        for (int i = 0; i < 16; i++)
        {
            newVibratorIntensities[i] = 0;
        }

        // get computed acceleration and jerk
        //Gforce = GetComponent<ACListener>().Gforce; // in a unit of G

        // convert them into planar value (ignore y component)
        planarGforce = -1 * GetComponent<ACListener>().Gforce;
        Gforce = new Vector3(planarGforce.x, 0, planarGforce.y);
        Gforce = Quaternion.Inverse(GetComponent<BackgroundVRListener>().HeadRotation) * Gforce;
        planarGforce = new Vector2(Gforce.x, Gforce.z);


        // Gforce interval: 0.15G ~ 1G
        planarGforceMagnitude = planarGforce.magnitude;
        GforceAngle = -Vector2.SignedAngle(planarGforce, new Vector2(0.0f, 1.0f));
        if (planarGforceMagnitude > 0.15f && inertiaFeedbackEnable)
        {
            inertiaing = true;
            started = true;
            convertGforce(GforceAngle, planarGforceMagnitude);
        }

        // check if nothing is happening

        if(!inertiaing && started)
        {
            // stop all vibrators
            started = false;
            gameObject.GetComponent<ArduinoSystem>().setAllToZero();
        }
    }

    void checkAngles()
    {
        // check if all angle values are zero
        float tmp = 0;
        for(int i = 0; i < 16; i++)
        {
            tmp += angleOfEachVibrator[i] * angleOfEachVibrator[i];

            // set all angles into -180 ~ 180
            while(angleOfEachVibrator[i] < -180.0f)
            {
                angleOfEachVibrator[i] += 360.0f;
            }
            while (angleOfEachVibrator[i] >= 180.0f)
            {
                angleOfEachVibrator[i] -= 360.0f;
            }
        }
        if(tmp == 0)
        {
            // if so, set to default values
            setDefaultAngle();
        }
    }
    void setDefaultAngle()
    {
        // 0 -> front
        // counterc lockwisely increase

        angleOfEachVibrator[0] = 0.0f;
        angleOfEachVibrator[1] = -22.5f;
        angleOfEachVibrator[2] = -45.0f;
        angleOfEachVibrator[3] = -67.5f;
        angleOfEachVibrator[4] = -90.0f;
        angleOfEachVibrator[5] = -112.5f;
        angleOfEachVibrator[6] = -135.0f;
        angleOfEachVibrator[7] = -157.5f;
        angleOfEachVibrator[8] = 180.0f;
        angleOfEachVibrator[9] = 157.5f;
        angleOfEachVibrator[10] = 135.0f;
        angleOfEachVibrator[11] = 112.5f;
        angleOfEachVibrator[12] = 90.0f;
        angleOfEachVibrator[13] = 67.5f;
        angleOfEachVibrator[14] = 45.0f;
        angleOfEachVibrator[15] = 22.5f;
    }

    private void convertGforce(float angle, float magnitude)
    {
        byte[] toArduinoBytes = new byte[3];
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(angle - angleOfEachVibrator[i]) < angleThreshold)
            {
                setVibratorIntensity(Mathf.Abs(angle - angleOfEachVibrator[i]), magnitude, i);
            }
            else if (Mathf.Abs(angle - angleOfEachVibrator[i] + 360.0f) < angleThreshold)
            {
                setVibratorIntensity(Mathf.Abs(angle - angleOfEachVibrator[i] + 360.0f), magnitude, i);
            }
            else if (Mathf.Abs(angle - angleOfEachVibrator[i] - 360.0f) < angleThreshold)
            {
                setVibratorIntensity(Mathf.Abs(angle - angleOfEachVibrator[i] - 360.0f), magnitude, i);
            }

            if (Mathf.Abs(newVibratorIntensities[i] - lastVibratorIntensities[i]) > intensityUpdateThreshold)
            {
                updateAmount++;
                
                toArduinoBytes[0] = System.Convert.ToByte((char)i);
                toArduinoBytes[1] = System.Convert.ToByte((char)(vibratorFrequency / 2));
                toArduinoBytes[2] = System.Convert.ToByte((char)newVibratorIntensities[i]);
                arduinoSystem.writeToArduinoByte(toArduinoBytes);

                lastVibratorIntensities[i] = newVibratorIntensities[i];
            }
            else if (lastVibratorIntensities[i] != 0 && newVibratorIntensities[i] == 0)
            {
                updateAmount++;

                toArduinoBytes[0] = System.Convert.ToByte((char)i);
                toArduinoBytes[1] = System.Convert.ToByte((char)(vibratorFrequency / 2));
                toArduinoBytes[2] = System.Convert.ToByte((char)newVibratorIntensities[i]);
                arduinoSystem.writeToArduinoByte(toArduinoBytes);

                lastVibratorIntensities[i] = newVibratorIntensities[i];
            }
        }
    }


    private void setVibratorIntensity(float angleDiff, float G_mag, int VibratorIndex)
    {
        newVibratorIntensities[VibratorIndex] = Mathf.FloorToInt(calculateIntensity(angleDiff, G_mag));
    }

    private float calculateIntensity(float angleDiff, float G_mag)
    {
        float peakIntensity = Mathf.Lerp(minIntensity, maxIntensity, (G_mag - GforceMinThreshold) / (GforceMaxThreshold - GforceMinThreshold));
        float ret = peakIntensity * (angleThreshold - angleDiff) / angleThreshold;
        return ret;
    }
}
