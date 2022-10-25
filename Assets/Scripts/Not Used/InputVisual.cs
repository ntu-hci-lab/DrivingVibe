using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;

public class InputVisual : MonoBehaviour
{
    public Transform token;
    public Transform Ltrigger;
    public Transform Rtrigger;
    public Image X;
    public Image Y;
    public Image A;
    public Image B;
    public Image Lshoulder;
    public Image Rshoulder;
    private XInputController gamepad;
    public float size = 100.0f;
    // Start is called before the first frame update
    void Start()
    {
        //gamepad = InputSystem.GetDevice<XInputController>();
        //InputSystem.GetDevice<Gamepad>();
        //gamepad = Gamepad.all[1] as XInputController;
    }

    // Update is called once per frame
    void Update()
    {
        Keyboard keyboard = InputSystem.GetDevice<Keyboard>();
        if (keyboard.spaceKey.isPressed)
        {
            gamepad = Gamepad.all[0] as XInputController;
        }

        if(gamepad != null)
        {
            token.localPosition = new Vector3(gamepad.leftStick.ReadUnprocessedValue().x * size, gamepad.leftStick.ReadUnprocessedValue().y * size, 0.0f);
            Ltrigger.localPosition = new Vector3(20.0f, gamepad.leftTrigger.ReadUnprocessedValue() * size, 0.0f);
            Rtrigger.localPosition = new Vector3(25.0f, gamepad.rightTrigger.ReadUnprocessedValue() * size, 0.0f);
            //Debug.Log($"Horizontal:{Input.GetAxis("Horizontal")} ,Vertical:{Input.GetAxis("Vertical")}");
            if (gamepad.aButton.isPressed)
            {
                A.color = Color.red;
            }
            else
            {
                A.color = Color.black;
            }

            if (gamepad.bButton.isPressed)
            {
                B.color = Color.red;
            }
            else
            {
                B.color = Color.black;
            }

            if (gamepad.xButton.isPressed)
            {
                X.color = Color.red;
            }
            else
            {
                X.color = Color.black;
            }

            if (gamepad.yButton.isPressed)
            {
                Y.color = Color.red;
            }
            else
            {
                Y.color = Color.black;
            }

            if (gamepad.leftShoulder.isPressed)
            {
                Lshoulder.color = Color.red;
            }
            else
            {
                Lshoulder.color = Color.black;
            }

            if (gamepad.rightShoulder.isPressed)
            {
                Rshoulder.color = Color.red;
            }
            else
            {
                Rshoulder.color = Color.black;
            }
        }


    }
}
