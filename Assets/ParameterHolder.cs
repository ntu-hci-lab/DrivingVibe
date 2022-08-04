using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParameterHolder : MonoBehaviour
{
    public float _timeStamp;
    public float[] _carCoordinates = new float[3];
    public float _speed;
    public float _acc_frontal;
    public float _acc_horizontal;
    public float _acc_vertical;
    //public float _gas;
    //public float _brake; //dead
    //public float _engineRPM; //dead
    //public int _gear; //dead
    public float[] _suspensionDiff = new float[4];
    //public float[] _bufferedSuspensionDiff = new float[4];
    public int _isTactileMotionOngoing;
    public int[] _directionalCueIntensity = new int[16];
    public int[] _RoadShakeIntensity = new int[16];
    public int[] _SumIntensity = new int[16];

    public float colorParameter;
}
