using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class CalibrationTest : MonoBehaviour
{

    public XInputController gamepad;

    public string RecordDir = "./record";
    public string RecordName = "test";
    public string CurrentRecordName = "currentRecord";
    public int baseIntensity = 20;
    public int stage;

    private WifiToArduino arduino;
    private string RecordPath;
    private string CurrentRecordPath;
    private RawImage[] Motor = new RawImage[16];
    private Text show;
    private int testIntensity;
    private InputField nameField;
    private Button submitBtn;
    private Button saveBtn;
    private Text targetPath;
    private Text stageText;
    private int[] singleWeighting = new int[16];
    private int[] singleWeighting2 = new int[16];
    private int[] sectionWeighting = new int[4];
    private int globalMultiplier = 100;
    private byte[] data = new byte[16];
    private bool isStarted = false;
    private bool isPatternPreview = false;

    //for stage 1
    private int phaseCtrl1;
    private int testCase;
    private static readonly int[] record = new int[16];
    private Color originColor;

    //for stage 2
    private static readonly int[] record2 = new int[2];
    public int stage2TestCase;
    private bool motionToggle;

    //for stage 3
    private Coroutine currentPatternPreview;

    //from pattern generator
    public float[] angleOfEachVibrator = new float[16];

    public float duration = 0.5f;

    private static Mutex mutex = new Mutex();
    private int worker = 0;

    private void Start()
    {
        gamepad = Gamepad.current as XInputController;
        try
        {
            if (!Directory.Exists(RecordDir))
            {
                Directory.CreateDirectory(RecordDir);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        for (int i = 0; i <= 15; ++i)
        {
            Motor[i] = GameObject.Find("M" + i.ToString()).GetComponent<RawImage>();
        }
        show = GameObject.Find("intensity Text").GetComponent<Text>();
        nameField = GameObject.Find("FilenameField").GetComponent<InputField>();
        submitBtn = GameObject.Find("SubmitBtn").GetComponent<Button>();
        saveBtn = GameObject.Find("SaveBtn").GetComponent<Button>();
        targetPath = GameObject.Find("TargetPath").GetComponent<Text>();
        RecordPath = Path.Combine(RecordDir, RecordName + ".txt");
        CurrentRecordPath = Path.Combine(RecordDir, CurrentRecordName + ".txt");
        targetPath.text = "save to: " + RecordPath;
        submitBtn.onClick.AddListener(submitBtnOnClick);
        saveBtn.onClick.AddListener(saveBtnOnClick);
        stageText = GameObject.Find("Stage2Text").GetComponent<Text>();
        originColor = GameObject.Find("Tag").GetComponent<RawImage>().color;
        arduino = GetComponent<WifiToArduino>();
        isStarted = false;
        // new 
        //int[] myIntensities = new int[16];
        //virtualLayer = GetComponent<VirtualLayer>();
        //myIntensities = virutalLayer.VirbratorIntensities;

        SetStart();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            isStarted = true;
        }else if (Input.GetKeyDown(KeyCode.Q))
        {
            isStarted = false;
        }

        if (!isStarted)
        {
            return;
        }

        if (stage == 1)
        {
            show.text = ((int)(testIntensity * 100 / baseIntensity)).ToString() + "%";
            if (worker == 0)
            {
                if (phaseCtrl1 == 0)
                {
                    StartCoroutine(doVibration(0, baseIntensity, duration));
                }
                else if (phaseCtrl1 == 2)
                {
                    StartCoroutine(doVibration(testCase, baseIntensity, duration));
                }
                else
                {
                    StartCoroutine(waiting(duration));
                }
                phaseCtrl1 = ++phaseCtrl1 % 4;
            }

            if (gamepad.dpad.up.wasPressedThisFrame)
            {
                if (testIntensity < 30)
                {
                    testIntensity += 2;
                }
                singleWeighting[testCase] = (int)(testIntensity * 100 / baseIntensity);
            }
            else if (gamepad.dpad.down.wasPressedThisFrame)
            {
                if (testIntensity > 2)
                {
                    testIntensity -= 2;
                }
                singleWeighting[testCase] = (int)(testIntensity * 100 / baseIntensity);
            }

            if (gamepad.dpad.right.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame)
            {
                if (testCase < 15)
                {
                    ++testCase;
                    testIntensity = (int)(baseIntensity * singleWeighting[testCase] / 100);
                    GameObject.Find("Tag").GetComponent<RectTransform>().anchoredPosition
                           = GameObject.Find(Motor[testCase].name).GetComponent<RectTransform>().anchoredPosition;
                }
                else
                {
                    GameObject.Find("Tag").GetComponent<RawImage>().color = Color.clear;
                    stage = 2;
                    stage2TestCase = 4;
                    testIntensity = (int)(baseIntensity * sectionWeighting[1] / 100);
                    //stageText.text = "increase to most bearable intensity";
                }
            }
            else if (gamepad.dpad.left.wasPressedThisFrame || gamepad.leftShoulder.wasPressedThisFrame || gamepad.leftTrigger.wasPressedThisFrame)
            {
                if (testCase > 1)
                {
                    --testCase;
                    testIntensity = (int)(baseIntensity * singleWeighting[testCase] / 100);
                    GameObject.Find("Tag").GetComponent<RectTransform>().anchoredPosition
                           = GameObject.Find(Motor[testCase].name).GetComponent<RectTransform>().anchoredPosition;
                }
            }
        }

        else if (stage == 2)
        {
            //show.text = ((int)(testIntensity * 100 / baseIntensity)).ToString() + "%";
            show.text = ((int)(testIntensity * 100 / baseIntensity)).ToString() + "%";
            if (worker == 0)
            {
                if (phaseCtrl1 == 0)
                {
                    StartCoroutine(doSectionVibrations(baseIntensity, duration, 0));
                }
                else if (phaseCtrl1 == 2)
                {
                    StartCoroutine(doSectionVibrations(baseIntensity, duration, stage2TestCase));
                }
                else
                {
                    StartCoroutine(waiting(duration));
                }
                phaseCtrl1 = ++phaseCtrl1 % 4;
            }

            if (gamepad.dpad.up.wasPressedThisFrame)
            {
                if (testIntensity < 30)
                {
                    testIntensity += 2;
                }
                sectionWeighting[stage2TestCase / 4] = (int)(testIntensity * 100 / baseIntensity);
                UpdateSingleWeighting2();
            }
            else if (gamepad.dpad.down.wasPressedThisFrame)
            {
                if (testIntensity > 2)
                {
                    testIntensity -= 2;
                }
                sectionWeighting[stage2TestCase / 4] = (int)(testIntensity * 100 / baseIntensity);
                UpdateSingleWeighting2();
            }

            if (gamepad.dpad.right.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame)
            {
                if (stage2TestCase < 12)
                {
                    stage2TestCase += 4;
                    testIntensity = (int)(baseIntensity * sectionWeighting[stage2TestCase / 4] / 100);
                }
                else
                {
                    //GameObject.Find("Tag").GetComponent<RawImage>().color = Color.clear;
                    stage = 3;
                    testIntensity = 100;
                    
                }
            }
            else if (gamepad.dpad.left.wasPressedThisFrame || gamepad.leftShoulder.wasPressedThisFrame || gamepad.leftTrigger.wasPressedThisFrame)
            {
                if (stage2TestCase > 4)
                {
                    stage2TestCase -= 4;
                    testIntensity = (int)(baseIntensity * sectionWeighting[stage2TestCase / 4] / 100);
                }
                else
                {
                    testIntensity = (int)(baseIntensity * singleWeighting[15] / 100);
                    stage = 1;
                    //testCase = 15;
                }
            }
        }

        else if (stage == 3)
        {
            show.text = testIntensity.ToString();

            if (gamepad.dpad.right.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame)
            {
                // cut the current preview
                if(currentPatternPreview != null)
                {
                    StopCoroutine(currentPatternPreview);
                }
                // play a new one
                currentPatternPreview =  StartCoroutine(patternPreview(testIntensity));
            }

            if (gamepad.dpad.up.wasPressedThisFrame)
            {
                if (testIntensity < 100)
                {
                    testIntensity += 10;
                }
                globalMultiplier = testIntensity;

                // cut the current preview
                if (currentPatternPreview != null)
                {
                    StopCoroutine(currentPatternPreview);
                }
                // play a new one
                currentPatternPreview = StartCoroutine(patternPreview(testIntensity));
            }
            if (gamepad.dpad.down.wasPressedThisFrame)
            {
                if (testIntensity > 80)
                {
                    testIntensity -= 10;
                }
                globalMultiplier = testIntensity;

                // cut the current preview
                if (currentPatternPreview != null)
                {
                    StopCoroutine(currentPatternPreview);
                }
                // play a new one
                currentPatternPreview = StartCoroutine(patternPreview(testIntensity));
            }
        }
    }

    private IEnumerator doVibration(int index, int intensity, float duration)
    {
        mutex.WaitOne();
        worker++;
        mutex.ReleaseMutex();

        //virtualHeadband.VibratorIntensities[index] = intensity * weighting[index] / 100;
        data[index] = System.Convert.ToByte((char)(intensity * singleWeighting2[index] * singleWeighting[index] / 10000));
        arduino.writeToArduinoByte(data);
        Motor[index].color = Color.black;

        yield return new WaitForSeconds(duration);
        data[index] = System.Convert.ToByte((char)0);
        arduino.writeToArduinoByte(data);
        //virtualHeadband.VibratorIntensities[index] = 0;
        Motor[index].color = Color.white;

        mutex.WaitOne();
        worker--;
        mutex.ReleaseMutex();
    }

    private IEnumerator patternPreview(int multiplier)
    {
        string path = Path.Combine(RecordDir, "VibrationForCalibration_v2.csv");
        FileInfo patternPreviewFile = new FileInfo(path);
        StreamReader reader = patternPreviewFile.OpenText();
        string line = reader.ReadLine();
        while(line != null)
        {
            parseLineToData(line, multiplier);
            arduino.writeToArduinoByte(data);
            yield return new WaitForFixedUpdate();
            line = reader.ReadLine();
        }
        for (int i = 0; i < 16; i++)
        {
            data[i] = System.Convert.ToByte((char)0);
        }
        arduino.writeToArduinoByte(data);
        isPatternPreview = false;
        yield break;
    }

    private void UpdateSingleWeighting2()
    {
        //section 0: 14,15,0,1,2
        //section 1: 2,3,4,5,6
        //section 2: 6,7,8,9,10
        //section 3: 10,11,12,13,14
        singleWeighting2[0] = sectionWeighting[0];
        singleWeighting2[1] = sectionWeighting[0];
        singleWeighting2[2] = (sectionWeighting[0] + sectionWeighting[1])/2;
        singleWeighting2[3] = sectionWeighting[1];
        singleWeighting2[4] = sectionWeighting[1];
        singleWeighting2[5] = sectionWeighting[1];
        singleWeighting2[6] = (sectionWeighting[1] + sectionWeighting[2]) / 2;
        singleWeighting2[7] = sectionWeighting[2];
        singleWeighting2[8] = sectionWeighting[2];
        singleWeighting2[9] = sectionWeighting[2];
        singleWeighting2[10] = (sectionWeighting[2] + sectionWeighting[3]) / 2;
        singleWeighting2[11] = sectionWeighting[3];
        singleWeighting2[12] = sectionWeighting[3];
        singleWeighting2[13] = sectionWeighting[3];
        singleWeighting2[14] = (sectionWeighting[3] + sectionWeighting[0]) / 2;
        singleWeighting2[15] = sectionWeighting[3];
    }

    private IEnumerator doSectionVibrations(int intensity, float duration, int offset)
    {

        mutex.WaitOne();
        worker++;
        mutex.ReleaseMutex();

        // offset: 0 or 4 or 8 or 12
        int intTmp = 0;
        for(int i = 0; i < 16; i++)
        {
            data[i] = System.Convert.ToByte((char)0);
        }
        for(int i = offset - 2; i <= offset + 2; i++)
        {
            int index = (i + 16) % 16;
            intTmp = Mathf.Min(40, (intensity * singleWeighting2[index] * singleWeighting[index] / 10000));
            data[index] = System.Convert.ToByte((char)(intTmp));
            Motor[index].color = Color.black;
        }
        arduino.writeToArduinoByte(data);

        yield return new WaitForSeconds(duration);
        for (int i = 0; i < 16; i++)
        {
            data[i] = System.Convert.ToByte((char)0);
            Motor[i].color = Color.white;
        }
        arduino.writeToArduinoByte(data);


        mutex.WaitOne();
        worker--;
        mutex.ReleaseMutex();

        yield break;
    }

    private IEnumerator waiting(float duration)
    {
        mutex.WaitOne();
        worker++;
        mutex.ReleaseMutex();

        yield return new WaitForSeconds(duration);

        mutex.WaitOne();
        worker--;
        mutex.ReleaseMutex();
    }

    private void SetStart()
    {
        stage = 1;
        phaseCtrl1 = 0;
        testCase = 1;
        stage2TestCase = 0;
        motionToggle = false;
        testIntensity = baseIntensity;
        for (int i = 0; i < 16; i++)
        {
            singleWeighting[i] = 100;
            singleWeighting2[i] = 100;
            data[i] = System.Convert.ToByte((char)0);
        }
        for (int i = 0; i < 4; i++)
        {
            sectionWeighting[i] = 100;
        }
        stageText.text = "";
        GameObject.Find("Tag").GetComponent<RectTransform>().anchoredPosition = GameObject.Find("M1").GetComponent<RectTransform>().anchoredPosition;
    }

    private void saveRecord()
    {
        int[] finalWeighting = new int[16];
        for(int i = 0; i < 16; i++)
        {
            finalWeighting[i] = singleWeighting[i] * singleWeighting2[i];
        }
        try
        {
            StreamWriter sw = new StreamWriter(RecordPath, false, Encoding.ASCII);
            var maxValue = findMaxInt(finalWeighting, 16);
            for (int k = 0; k < 16; ++k)  //with motor 0
            {
                sw.WriteLine(Mathf.Floor(100 * finalWeighting[k] / maxValue).ToString());
            }
            sw.WriteLine(globalMultiplier.ToString());
            
            sw.Close();
            File.Copy(RecordPath, CurrentRecordPath, true);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        Debug.Log(RecordPath + " saved!");
    }

    public void submitBtnOnClick()
    {
        RecordName = nameField.text;
        RecordPath = Path.Combine(RecordDir, RecordName + ".txt");
        targetPath.text = "save to: " + RecordPath;
    }

    private void saveBtnOnClick()
    {
        saveRecord();
    }

    private void clearMotors()
    {
        for (int i = 0; i < 16; ++i)
        {
            Motor[i].color = Color.white;
        }
    }

    private void parseLineToData(string line, int multi)
    {
        string[] VibrationRecords = line.Split(',');
        for(int i = 0; i < 16; i++)
        {
            int intTmp = int.Parse(VibrationRecords[i]) * multi * singleWeighting[i] / 100 / 100;
            intTmp = Mathf.Min(intTmp, 40);
            data[i] = System.Convert.ToByte((char)(intTmp));
        }
    }

    private int findMaxInt(int[] array, int length)
    {
        int ret = 0;
        for(int i = 0; i < length; i++)
        {
            if(ret < array[i])
            {
                ret = array[i];
            }
        }
        return ret;
    }
}
