using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using AirDriVR;

public class PositionCopier : MonoBehaviour
{
    [Header("Track related")]
    public string RecordDir;
    public int fileCount = 4;
    public GameObject token;
    public float resolutionInTime = 0.5f;
    public Transform SphereHolder;
    public float renderingLength = 150.0f;
    public float tokenSize = 30.0f;
    private List<GameObject> tokenList = new List<GameObject>();

    [Header("Draw Options")]
    public ColoringMethod coloringMethod;
    public float coloringUpperBound;
    public float coloringLowerBound;
    public Color PositiveMaxColor;
    public Color NegetiveMaxColor;

    public enum ColoringMethod
    {
        Speed,
        Acc_frontal,
        Acc_horizontal,
        EngineRPM,
        isTactileMotionOngoing,
        SuspensionVelo,
        CueIntensity,
        ShakeIntensity,
        SumIntensity
    };
    private float _timeStamp;

    void Start()
    {
        trailGenerate();
        UpdateTokenColor();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpdateTokenColor();
        }
    }

    public void UpdateTokenColor()
    {
        int SphereCount = SphereHolder.childCount;
        float parameter;
        GameObject sphere;
        for (int i = 0; i < SphereCount; i++)
        {
            sphere = SphereHolder.GetChild(i).gameObject;

            sphere.transform.localScale = new Vector3(tokenSize, tokenSize, tokenSize);

            switch (coloringMethod)
            {
                case ColoringMethod.Speed:
                    parameter = sphere.GetComponent<ParameterHolder>()._speed;
                    break;
                case ColoringMethod.Acc_frontal:
                    parameter = sphere.GetComponent<ParameterHolder>()._acc_frontal;
                    break;
                case ColoringMethod.Acc_horizontal:
                    parameter = sphere.GetComponent<ParameterHolder>()._acc_horizontal;
                    break;
                /*
                case ColoringMethod.EngineRPM:
                    parameter = sphere.GetComponent<ParameterHolder>()._engineRPM;
                    break;
                */

                case ColoringMethod.isTactileMotionOngoing:
                    parameter = sphere.GetComponent<ParameterHolder>()._isTactileMotionOngoing;
                    //coloringLowerBound = 0.0f;
                    //coloringUpperBound = 1.0f;
                    break;
                case ColoringMethod.SuspensionVelo:
                    parameter = sphere.GetComponent<ParameterHolder>()._suspensionDiff[0];
                    //coloringLowerBound = 0.0f;
                    //coloringUpperBound = 0.4f;
                    break;
                case ColoringMethod.CueIntensity:
                    parameter = 0;
                    for (int j = 0; j < 16; j++)
                    {
                        parameter += sphere.GetComponent<ParameterHolder>()._directionalCueIntensity[j];
                    }
                    //coloringLowerBound = 0.0f;
                    //coloringUpperBound = 1600.0f;
                    break;
                case ColoringMethod.ShakeIntensity:
                    parameter = 0;
                    for (int j = 0; j < 16; j++)
                    {
                        parameter += sphere.GetComponent<ParameterHolder>()._RoadShakeIntensity[j];
                    }
                    //coloringLowerBound = 0.0f;
                    //coloringUpperBound = 1200.0f;
                    break;
                case ColoringMethod.SumIntensity:
                    parameter = 0;
                    for(int j = 0; j < 16; j++)
                    {
                        parameter += sphere.GetComponent<ParameterHolder>()._SumIntensity[j];
                    }
                    //coloringLowerBound = 0.0f;
                    //coloringUpperBound = 1600.0f;
                    break;
                default:
                    parameter = 0;
                    break;
            }

            sphere.GetComponent<ParameterHolder>().colorParameter = parameter;
            if (parameter > 0)
            {
                sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.white, PositiveMaxColor, Mathf.InverseLerp(coloringLowerBound, coloringUpperBound, parameter)));
            }
            else
            {
                sphere.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.white, NegetiveMaxColor, Mathf.InverseLerp(coloringLowerBound, coloringUpperBound, -parameter)));
            }

            
        }
    }

    private void trailGenerate()
    {
        _timeStamp = 0;
        string fileName;
        string path;
        FileInfo patternPreviewFile;
        StreamReader reader;
        string dataLine;
        for (int i = 0; i < fileCount; i++)
        {
            // open the file
            fileName = "Profile_minute" + i.ToString() + ".csv";
            path = Path.Combine(RecordDir, fileName);
            patternPreviewFile = new FileInfo(path);
            reader = patternPreviewFile.OpenText();
            dataLine = reader.ReadLine(); // skip the first line

            dataLine = reader.ReadLine();
            while (dataLine != null)
            {
                parseLine(dataLine);
                dataLine = reader.ReadLine();
            }
            reader.Close();
        }

        return;
    }
    private void parseLine(string line)
    {
        /*
        private float _timeStamp;
        private float[] _carCoordinates = new float[3];
        private float _speed;
        private float _acc_frontal;
        private float _acc_horizontal;
        private float gas;
        private float[] _suspensionDiff = new float[4];
        private int _isTactileMotionOngoing;
        private int[] _directionalCueIntensity = new int[16];
        private int[] _RoadShakeIntensity = new int[16];
        private int[] _SumIntensity = new int[16];
        */
        string[] data = line.Split(',');

        float dataTimeStamp = float.Parse(data[0]);
        if(dataTimeStamp > renderingLength)
        {
            return;
        }
        if (_timeStamp + resolutionInTime < dataTimeStamp)
        {
            _timeStamp = dataTimeStamp;
            // sufficient time skip, make a sphere with data in it
            Vector3 pos = new Vector3(float.Parse(data[1]), -50.0f, float.Parse(data[3]));
            GameObject currentToken = Instantiate(token, pos, Quaternion.identity, SphereHolder);
            tokenList.Add(currentToken);
            currentToken.transform.localScale = new Vector3(tokenSize, tokenSize, tokenSize);
            ParameterHolder parameterHolder = token.GetComponent<ParameterHolder>();

            // dump the data
            parameterHolder._timeStamp = float.Parse(data[0]);
            parameterHolder._carCoordinates[0] = float.Parse(data[1]);
            parameterHolder._carCoordinates[1] = float.Parse(data[2]);
            parameterHolder._carCoordinates[2] = float.Parse(data[3]);
            parameterHolder._speed = float.Parse(data[4]);
            parameterHolder._acc_frontal = float.Parse(data[5]);
            parameterHolder._acc_horizontal = float.Parse(data[6]);
            parameterHolder._gas = float.Parse(data[7]);
            parameterHolder._suspensionDiff[0] = float.Parse(data[8]);
            parameterHolder._suspensionDiff[1] = float.Parse(data[9]);
            parameterHolder._suspensionDiff[2] = float.Parse(data[10]);
            parameterHolder._suspensionDiff[3] = float.Parse(data[11]);
            parameterHolder._isTactileMotionOngoing = int.Parse(data[12]);
            for(int i = 0; i < 16; i++)
            {
                // data[13] ~ data[28]
                parameterHolder._directionalCueIntensity[i] = int.Parse(data[i + 13]);
                // data[29] ~ data[44]
                parameterHolder._RoadShakeIntensity[i] = int.Parse(data[i + 29]);
                // data[45] ~ data[60]
                parameterHolder._SumIntensity[i] = int.Parse(data[i + 45]);
            }
        }
    }
    private void DeleteCurrentTrack()
    {
        foreach(GameObject t in tokenList)
        {
            if(t != null)
            {
                Destroy(t);
            }
        }
        tokenList.Clear();
    }

    public void RegenerateTrack()
    {
        DeleteCurrentTrack();
        trailGenerate();
    }
}




[CustomEditor(typeof(PositionCopier))]
public class AddButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PositionCopier positionCopier = (PositionCopier)target;

        if (GUILayout.Button("Regenerate Track"))
        {
            positionCopier.RegenerateTrack();
            positionCopier.UpdateTokenColor();
        }
        if (GUILayout.Button("UpdateColor"))
        {
            positionCopier.UpdateTokenColor();
        }

    }
}