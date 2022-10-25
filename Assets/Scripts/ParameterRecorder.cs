using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using AirDriVR;
using UnityEditor;

public class ParameterRecorder : MonoBehaviour
{
    public string FilePath = @"D:\vibrateRecords\";
    //public string FileName = "0.csv";
    public float recordTimer = 0.0f;
    public float recordLength = 10.0f;
    public int recordMinutes;
    private bool isRecording = false;
    private float[] data = new float[7];
    public int recordCount = 0;
    private ACListener listener;
    private PatternGenerator patternGenerator;
    private VirtualHeadband virtualHeadband;
    private ControllerHaptic controllerHaptic;

    // datas
    private float _timeStamp;
    private float[] _carCoordinates = new float[3];
    private float _speed;
    private float _acc_frontal;
    private float _acc_horizontal;
    private float _gas;
    private float[] _suspensionDiff = new float[4];
    private int isTactileMotionOngoing;
    private int _leftMotor;
    private int _rightMotor;
    private int[] _directionalCueIntensity = new int[16];
    private int[] _RoadShakeIntensity = new int[16];
    private int[] _SumIntensity = new int[16];

    // Start is called before the first frame update
    void Start()
    {
        //recordCount = 0;
        for(int i = 0; i < 7; i++)
        {
            data[0] = 0.0f;
        }
        listener = GetComponent<ACListener>();
        patternGenerator = GetComponent<PatternGenerator>();
        virtualHeadband = GetComponent<VirtualHeadband>();
        controllerHaptic = GetComponent<ControllerHaptic>();
        //suspensionDiff = listener.suspensionDiff;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Recording()
    {
        if (!isRecording)
        {
            isRecording = true;
            recordMinutes = (int)recordLength / 60;
            StartCoroutine(StartRecording());
        }
        else
        {
            Debug.Log("Currently recording!");
        }
    }

    private IEnumerator StartRecording()
    {
        Debug.Log("Start recording parameters!");
        string FileName;
        StreamWriter Writer;
        string headerLine = "_timeStamp,_carCoordinates_x,_carCoordinates_y,_carCoordinates_z,_speed,_acc_frontal,_acc_horizontal,_gas,_suspensionDiff_FL,_suspensionDiff_FR,_suspensionDiff_BL,_suspensionDiff_BR,isTactileMotionOngoing,_leftMotor,_rightMotor,Cue0,Cue1,Cue2,Cue3,Cue4,Cue5,Cue6,Cue7,Cue8,Cue9,Cue10,Cue11,Cue12,Cue13,Cue14,Cue15,Shake0,Shake1,Shake2,Shake3,Shake4,Shake5,Shake6,Shake7,Shake8,Shake9,Shake10,Shake11,Shake12,Shake13,Shake14,Shake15,Sum0,Sum1,Sum2,Sum3,Sum4,Sum5,Sum6,Sum7,Sum8,Sum9,Sum10,Sum11,Sum12,Sum13,Sum14,Sum15"; ;
        float timer;
        recordTimer = 0.0f;

        for (int i = 0; i < recordMinutes; i++)
        {
            FileName = FilePath + "Profile_minute" + i.ToString() + ".csv";
            Writer = new StreamWriter(FileName);

            Writer.WriteLine(headerLine);

            timer = 0.0f;
            while (timer < 60)
            {
                GetParameter();
                WriteWithCSVFormat(Writer);
                recordTimer += Time.fixedDeltaTime;
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            Writer.Close();
        }


        FileName = FilePath + "Profile_minute" + recordMinutes.ToString() + ".csv";
        Writer = new StreamWriter(FileName);
        Writer.WriteLine(headerLine);
        while (recordTimer < recordLength)
        {
            GetParameter();
            WriteWithCSVFormat(Writer);
            recordTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        Writer.Close();

        isRecording = false;
        Debug.Log("Record finished!");

        yield break;
    }

    private void GetParameter()
    {
        /*
        private float _timeStamp;
        private float[] _carCoordinates = new float[3];
        private float _speed;
        private float _acc_frontal;
        private float _acc_horizontal;
        private float _gas;
        private float[] _suspensionDiff = new float[4];
        private int isTactileMotionOngoing;
        private int _leftMotor;
        private int _rightMotor;
        private int[] _directionalCueIntensity = new int[16];
        private int[] _RoadShakeIntensity = new int[16];
        private int[] _SumIntensity = new int[16];
        */
        _timeStamp = Time.fixedTime;
        for(int i = 0; i < 3; i++)
        {
            _carCoordinates[i] = listener.pos[i];
        }
        _speed = listener.velocity;
        _acc_frontal = listener.Gforce.y;
        _acc_horizontal = listener.Gforce.x;
        _gas = listener.gas;
        for (int i = 0; i < 4; i++)
        {
            _suspensionDiff[i] = listener.suspensionDiff[i];
            //_suspensionDiff[i] = listener.unBufferedSuspensionDiff[i];
        }
        _leftMotor = controllerHaptic.left;
        _rightMotor = controllerHaptic.right;

        for (int i = 0; i < 16; i++)
        {
            _directionalCueIntensity[i] = patternGenerator.DirectionalCueIntensities[i];
            _RoadShakeIntensity[i] = patternGenerator.RoadShakeIntensities[i];
            _SumIntensity[i] = virtualHeadband.HeadbandIntensity[i];
        }
        if (patternGenerator.isTactileMotionOngoing)
        {
            isTactileMotionOngoing = 1;
        }
        else
        {
            isTactileMotionOngoing = 0;
        }
    }
    private void WriteWithCSVFormat(StreamWriter Writer)
    {
        /*
        private float _timeStamp;
        private float[] _carCoordinates = new float[3];
        private float _speed;
        private float _acc_frontal;
        private float _acc_horizontal;
        private float _gas;
        private float[] _suspensionDiff = new float[4];
        private int isTactileMotionOngoing;
        private int _leftMotor;
        private int _rightMotor;
        private int[] _directionalCueIntensity = new int[16];
        private int[] _RoadShakeIntensity = new int[16];
        private int[] _SumIntensity = new int[16];
        */
        string line = _timeStamp.ToString() + ",";
        line = line + _carCoordinates[0].ToString() + "," + _carCoordinates[1].ToString() + "," + _carCoordinates[2].ToString() + ",";
        line = line + _speed.ToString() + ",";
        line = line + _acc_frontal.ToString() + ",";
        line = line + _acc_horizontal.ToString() + ",";
        line = line + _gas.ToString() + ",";
        line = line + _suspensionDiff[0].ToString() + "," + _suspensionDiff[1].ToString() + "," + _suspensionDiff[2].ToString() + "," + _suspensionDiff[3].ToString() + ",";
        line = line + isTactileMotionOngoing.ToString() + ",";
        line = line + _leftMotor.ToString() + "," + _rightMotor.ToString() + ",";
        for (int i = 0; i < 16; i++)
        {
            line = line + _directionalCueIntensity[i].ToString() + ",";
        }
        for (int i = 0; i < 16; i++)
        {
            line = line + _RoadShakeIntensity[i].ToString() + ",";
        }
        for (int i = 0; i < 16; i++)
        {
            line = line + _SumIntensity[i].ToString() + ",";
        }
        //Debug.Log(line);
        Writer.WriteLine(line); 
    }
}

[CustomEditor(typeof(ParameterRecorder))]
public class RecorderBtn : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ParameterRecorder recorder = (ParameterRecorder)target;

        if (GUILayout.Button("Start Recording"))
        {
            recorder.Recording();
        }
    }
}