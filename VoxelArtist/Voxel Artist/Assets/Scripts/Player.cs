using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //Movement
    private Vector3 thrustVector;
    private float thrustSpeed = 10f;

    //Settings
    private readonly float MOUSE_SENS_COEFF = 1f;
    private float mouseSensitivity = 0.6f;

    //Camera
    private float hFieldOfView = 103f;
    private const float H_FIELD_OF_VIEW_MIN = 80f;
    private const float H_FIELD_OF_VIEW_MAX = 103f;
    private float fpCamPitch = 0f;
    private float fpCamYaw = 0f;
    private float fpCamRoll = 0f;

    void Start()
    {
        //Camera HFOV
        if (hFieldOfView > H_FIELD_OF_VIEW_MIN && hFieldOfView < H_FIELD_OF_VIEW_MAX)
        {
            //Camera class's fieldOfView field stores VERTICAL field of view, so we convert h to v
            Camera.main.fieldOfView = Camera.HorizontalToVerticalFieldOfView(hFieldOfView, Camera.main.GetComponent<Camera>().aspect);
        }

        //Clip planes
        Camera.main.nearClipPlane = 0.05f;
        Camera.main.farClipPlane = 500f;
    }

    private void Update()
    {
        //Camera view
        //Pitch
        fpCamPitch -= Input.GetAxisRaw("Mouse Y") * mouseSensitivity * MOUSE_SENS_COEFF;
        //Yaw
        if (fpCamPitch >= 90 && fpCamPitch < 270)
        {
            //Normal
            fpCamYaw -= Input.GetAxisRaw("Mouse X") * mouseSensitivity * MOUSE_SENS_COEFF;
        }
        else
        {
            //Inverted
            fpCamYaw += Input.GetAxisRaw("Mouse X") * mouseSensitivity * MOUSE_SENS_COEFF;
        }
        //Roll
        fpCamRoll = 0f;

        //LOOP ANGLE
        LoopEulerAngle(fpCamYaw);
        LoopEulerAngle(fpCamPitch);
        LoopEulerAngle(fpCamRoll);

        //CLAMP ANGLE
        fpCamPitch = Mathf.Clamp(fpCamPitch, -90f, 89f);

        //APPLY ROTATION
        Camera.main.transform.localRotation = Quaternion.Euler(fpCamPitch, fpCamYaw, 0f);
    }

    void FixedUpdate()
    {
        //Movement
        thrustVector = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) thrustVector += transform.forward;
        if (Input.GetKey(KeyCode.S)) thrustVector += -transform.forward;
        if (Input.GetKey(KeyCode.A)) thrustVector += -transform.right;
        if (Input.GetKey(KeyCode.D)) thrustVector += transform.right;
        if (Input.GetKey(KeyCode.Space)) thrustVector += transform.up;
        if (Input.GetKey(KeyCode.LeftControl)) thrustVector += -transform.up;

        if (thrustVector != Vector3.zero)
        {
            transform.position += thrustVector.normalized * thrustSpeed * Time.fixedDeltaTime;
        }
    }

    private float LoopEulerAngle(float angle)
    {
        if (angle >= 360) angle -= 360;
        else if (angle < 0) angle += 360;

        return angle;
    }
}