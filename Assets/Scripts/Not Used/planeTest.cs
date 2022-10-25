using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirDriVR;

public class planeTest : MonoBehaviour
{
    public float carLength;
    public float carWidth;
    public float scaleFactor;

    public float[] suspension = new float[4];

    private Vector3 frontRightWheel;
    private Vector3 frontLeftWheel;
    private Vector3 backRightWheel;
    private Vector3 backLeftWheel;


    public Plane[] carPlanes = new Plane[4];
    public Vector3[] planeNormals = new Vector3[4];

    // Update is called once per frame
    void Update()
    {

        backRightWheel = new Vector3(carWidth / 2, suspension[0] * scaleFactor, -carLength / 2);
        backLeftWheel = new Vector3(-carWidth / 2, suspension[1] * scaleFactor, -carLength / 2);
        frontRightWheel = new Vector3(carWidth / 2, suspension[2] * scaleFactor, carLength / 2);
        frontLeftWheel = new Vector3(-carWidth / 2, suspension[3] * scaleFactor, carLength / 2);

        carPlanes[0] = new Plane(backLeftWheel, frontLeftWheel, frontRightWheel);
        planeNormals[0] = carPlanes[0].normal;
        carPlanes[1] = new Plane(frontLeftWheel, frontRightWheel, backRightWheel);
        planeNormals[1] = carPlanes[1].normal;
        carPlanes[2] = new Plane(backRightWheel, backLeftWheel, frontLeftWheel);
        planeNormals[2] = carPlanes[2].normal;
        carPlanes[3] = new Plane(frontRightWheel, backRightWheel, backLeftWheel);
        planeNormals[3] = carPlanes[3].normal;
    }
}
