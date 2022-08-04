using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using AirDriVR;

public class CustomRecorder : MonoBehaviour
{
    public string FilePath = @"D:\vibrateRecords\";
    //public string FileName = "0.csv";
    public float recordLength = 10.0f;
    public int recordMinutes;
    private bool isRecording = false;
    public int recordCount = 0;
    private ACListener listener;
    //private ControllerTest controllerHaptic;

    // datas
    private float _timeStamp;
    private float _speed;
    private float _acc;
    private float _acc_frontal;
    private float _acc_horizontal;
    private float _acc_vertical;
    private float _gas;
    private byte _leftMotor;
    private byte _rightMotor;
    private float _carPosZ;
    private float[] _suspension = new float[4];
    private int _laptime;
    private float[] _suspensionDatas = new float[4];

    // Start is called before the first frame update
    void Start()
    {
        listener = GetComponent<ACListener>();
        //controllerHaptic = GetComponent<ControllerTest>();
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
        string headerLine = "_timeStamp,_acc,_acc_frontal,_acc_horizontal,_susDiffLF,_susDiffRF,_susDiffLB,_susDiffRB"; ;
        float timer;
        float timeStamp = 0;

        for (int i = 0; i < recordMinutes; i++)
        {
            FileName = FilePath + "Suspension_Profile_minute" + i.ToString() + ".csv";
            Writer = new StreamWriter(FileName);

            Writer.WriteLine(headerLine);

            timer = 0.0f;
            while (timer < 60)
            {
                GetParameter();
                WriteWithCSVFormat(Writer);
                timeStamp += Time.fixedDeltaTime;
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            Writer.Close();
        }


        FileName = FilePath + "Suspension_Profile_minute" + recordMinutes.ToString() + ".csv";
        Writer = new StreamWriter(FileName);
        Writer.WriteLine(headerLine);
        while (timeStamp < recordLength)
        {
            GetParameter();
            WriteWithCSVFormat(Writer);
            timeStamp += Time.fixedDeltaTime;
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
        private float _speed;
        private float _acc_frontal;
        private float _acc_horizontal;
        private float _acc_vertical;
        private float _gas;
        private float _brake;
        private float _engineRPM;
        private byte leftMotor;
        private byte rightMotor;
        */
        _timeStamp = Time.fixedTime;

        //_speed = listener.velocity;
        _acc = listener.Gforce.magnitude;
        _acc_frontal = listener.Gforce.y;
        _acc_horizontal = listener.Gforce.x;
        //_acc_vertical = listener.G_vertical;
        //_gas = listener.gas;
        //_suspension = listener.newSuspension;
        _laptime = listener.lastRecordTime;
        _suspensionDatas[0] = listener.suspensionDiff[0];
        _suspensionDatas[1] = listener.suspensionDiff[1];
        _suspensionDatas[2] = listener.suspensionDiff[2];
        _suspensionDatas[3] = listener.suspensionDiff[3];
    }
    private void WriteWithCSVFormat(StreamWriter Writer)
    {
        /*
        private float _timeStamp;
        private float[] _carCoordinates = new float[3];
        private float _speed;
        private float _acc_frontal;
        private float _acc_horizontal;
        private float _acc_vertical;
        private float _gas;
        private float _brake;
        private float _engineRPM;
        private float _gear;
        private float[] _suspensionDiff = new float[4];
        private int isTactileMotionOngoing;
        private int[] _directionalCueIntensity = new int[16];
        private int[] _RoadShakeIntensity = new int[16];
        private int[] _SumIntensity = new int[16];
        */
        string line = _timeStamp.ToString() + ",";
        line = line + _acc.ToString() + "," + _acc_frontal.ToString() + "," + _acc_horizontal.ToString() + ",";
        line = line + _suspensionDatas[0].ToString() + "," + _suspensionDatas[1].ToString() + "," + _suspensionDatas[2].ToString() + "," + _suspensionDatas[3].ToString();
        //Debug.Log(line);
        Writer.WriteLine(line);
    }
}

[CustomEditor(typeof(CustomRecorder))]
public class RecorderBtns : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        CustomRecorder recorder = (CustomRecorder)target;

        if (GUILayout.Button("Start Recording"))
        {
            recorder.Recording();
        }
    }
}