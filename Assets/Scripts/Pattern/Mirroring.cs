using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using System.Threading;

public class Mirroring : Pattern
{
    [Header("Controller Motor Intensity")]
    public byte left;
    public byte right;

    protected ViGEmClient client;
    protected IXbox360Controller _fakeController;
    public XInputController gamepad;
    //protected byte LeftMotor, RightMotor;
    //protected short LeftThumbX, LeftThumbY, RightThumbX, RightThumbY;
    //public byte LeftTrigger, RightTrigger;
    //protected bool Y, B, A, X, Up, Right, Down, Left;
    //protected bool LeftThumb, RightThumb, StartBtn, BackBtn, LeftShoulder, RightShoulder;

    // A: Btn0, B: Btn1, X: Btn2, Y: Btn3, LeftShoulder: Btn4, RightShoulder: Btn5

    void Start()
    {
        client = new ViGEmClient();

        // create a virtual controller as the first controller
        _fakeController = client.CreateXbox360Controller();
        _fakeController.Connect();

        // handler for feedback event
        _fakeController.FeedbackReceived += Controller_FeedbackReceived;

        //Gamepad.all[1].MakeCurrent();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Gamepad.all.Count);
        if (gamepad != null)
        {
            ProcessPhysicalState();
        }
    }
    public void Controller_FeedbackReceived(object sender, Xbox360FeedbackReceivedEventArgs e)
    {
        if (gamepad != null)
        {
            UnityMainThread.wkr.AddJob(() =>
            {
                // Will run on main thread, hence issue is solved
                gamepad.SetMotorSpeeds(e.LargeMotor / 255.0f, e.SmallMotor / 255.0f);
                left = e.LargeMotor;
                right = e.SmallMotor;
                float percentageIntensity = (float)(left + right) / 512.0f;
                for (int i = 0; i < 16; i++)
                {
                    HeadbandIntensities[i] = Mathf.CeilToInt(percentageIntensity * 100);
                }
            });
        }
    }

    public void ProcessPhysicalState()
    {
        // Buttons: Y, B, A, X, Up, Right, Down, Left, LeftThumb, RightThumb, Start, Back, LeftShoulder, RightShoulder;
        // Y, B, A, X
        _fakeController.SetButtonState(Xbox360Button.Y, gamepad.yButton.isPressed);
        _fakeController.SetButtonState(Xbox360Button.B, gamepad.bButton.isPressed);
        _fakeController.SetButtonState(Xbox360Button.A, gamepad.aButton.isPressed);
        _fakeController.SetButtonState(Xbox360Button.X, gamepad.xButton.isPressed);
        // LeftShoulder, RightShoulder
        _fakeController.SetButtonState(Xbox360Button.LeftShoulder, gamepad.leftShoulder.isPressed);
        _fakeController.SetButtonState(Xbox360Button.RightShoulder, gamepad.rightShoulder.isPressed);


        // Axis: LeftThumbX, LeftThumbY, RightThumbX, RightThumbY
        short axisValue;
        axisValue = (short)Mathf.FloorToInt(gamepad.leftStick.ReadUnprocessedValue().x * 32767.0f);
        _fakeController.SetAxisValue(Xbox360Axis.LeftThumbX, axisValue);
        axisValue = (short)Mathf.FloorToInt(gamepad.leftStick.ReadUnprocessedValue().y * 32767.0f);
        _fakeController.SetAxisValue(Xbox360Axis.LeftThumbY, axisValue);
        axisValue = (short)Mathf.FloorToInt(gamepad.rightStick.ReadUnprocessedValue().x * 32767.0f);
        _fakeController.SetAxisValue(Xbox360Axis.RightThumbX, axisValue);
        axisValue = (short)Mathf.FloorToInt(gamepad.rightStick.ReadUnprocessedValue().y * 32767.0f);
        _fakeController.SetAxisValue(Xbox360Axis.RightThumbY, axisValue);

        // Slider: LeftTriggerX, RightTrigger
        byte sliderValue;
        sliderValue = (byte)Mathf.FloorToInt(gamepad.leftTrigger.ReadUnprocessedValue() * 255.0f);
        _fakeController.SetSliderValue(Xbox360Slider.LeftTrigger, sliderValue);
        sliderValue = (byte)Mathf.FloorToInt(gamepad.rightTrigger.ReadUnprocessedValue() * 255.0f);
        _fakeController.SetSliderValue(Xbox360Slider.RightTrigger, sliderValue);

        //Gamepad.all[0].SetMotorSpeeds(gamepad.leftTrigger.ReadUnprocessedValue(), gamepad.rightTrigger.ReadUnprocessedValue());

        _fakeController.SubmitReport();
    }

    public void GetPhysicalController()
    {
        gamepad = Gamepad.all[Gamepad.all.Count - 1] as XInputController;
        if (gamepad != null)
        {
            Debug.Log("Physical controller connected!");
        }
        //Gamepad.all[1].MakeCurrent();
    }

    public void DisconnectVirtualController()
    {
        _fakeController.Disconnect();
    }

    private void OnApplicationQuit()
    {
        DisconnectVirtualController();
    }
}

[CustomEditor(typeof(Mirroring))]
public class MirroringBtn : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Mirroring mirroring = (Mirroring)target;

        if (GUILayout.Button("GetPhysicalController"))
        {
            mirroring.GetPhysicalController();
        }
        if (GUILayout.Button("DisconnectVirtualController"))
        {
            mirroring.DisconnectVirtualController();
        }
    }
}