using UnityEngine;
using System;
using System.Collections;

public class ExtendedFlyCam : MonoBehaviour
{
    public float cameraSensitivity = 90;
    public float normalMoveSpeed = 10;
    public float climbSpeed = 4;
    public float slowMoveFactor = 0.25f;
    public float fastMoveFactor = 3;
    public bool lockRotation = false;
    public bool mouse3D = true;
    public bool dynamicFOV = true;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    //private bool isSlowMo = false;
    //private float moveSpeedFast;
    //private float climbSpeedFast;
    //private float moveSpeedNormal;
    //private float climbSpeedNormal;
    //private float cameraSensitivityFast;
    //private float cameraSensitivityNormal;
    private Camera cam;


    void Start ()
    {
        //moveSpeedFast = normalMoveSpeed * 15f;
        //climbSpeedFast = climbSpeed * 15f;
        //moveSpeedNormal = normalMoveSpeed;
        //climbSpeedNormal = climbSpeed;
        //cameraSensitivityFast = cameraSensitivity * 15f;
        //cameraSensitivityNormal = cameraSensitivity;

        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update ()
    {
        //if(Input.GetKeyDown(KeyCode.Space))
            //isSlowMo = !isSlowMo;
        
        if(!lockRotation)
        {
            rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
            rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
            rotationY = Mathf.Clamp (rotationY, -90, 90);

            transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

            if (Input.GetMouseButton(0))
                transform.position += transform.up * climbSpeed * Time.deltaTime * 3f;

            if (Input.GetMouseButton(1))
                transform.position -= transform.up * climbSpeed * Time.deltaTime * 3f;

            if(dynamicFOV)
            {
                float height = (Mathf.InverseLerp(-8000f, 0f, transform.position.y) * 20f) + 35f;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, Mathf.Clamp(height, 35f, 55f), Time.deltaTime);
            }

            if (mouse3D)
            {
                if(transform.localEulerAngles.y > 180)
                    transform.position += (-transform.right * transform.localEulerAngles.y / 120f) * normalMoveSpeed * Time.deltaTime;
                else if (transform.localEulerAngles.y < 180)
                    transform.position += (transform.right * transform.localEulerAngles.y / 120f) * normalMoveSpeed * Time.deltaTime;

                transform.position += (transform.forward * (Mathf.Abs(transform.localEulerAngles.x) / 120f)) * normalMoveSpeed * Time.deltaTime;
            }
        }

        //if(isSlowMo)
        //{
        //    normalMoveSpeed = moveSpeedFast;
        //    climbSpeed = climbSpeedFast;
        //    cameraSensitivity = cameraSensitivityFast;
        //    Time.timeScale = 0.1f;
        //}   
        //else
        //{
        //    normalMoveSpeed = moveSpeedNormal;
        //    climbSpeed = climbSpeedNormal;
        //    cameraSensitivity = cameraSensitivityNormal;
        //    Time.timeScale = 1.0f;
        //}
        
        if (Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift))
        {
            transform.position += transform.forward * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * fastMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else if (Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt))
        {
            transform.position += transform.forward * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * (normalMoveSpeed * slowMoveFactor) * Input.GetAxis("Horizontal") * Time.deltaTime;
        }
        else
        {
            transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
            transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
        }

        if (Input.GetKey (KeyCode.E))
            transform.position += transform.up * climbSpeed * Time.deltaTime;
        
        if (Input.GetKey (KeyCode.Q))
            transform.position -= transform.up * climbSpeed * Time.deltaTime;

        //if (Input.GetKeyDown (KeyCode.End))
            //Screen.lockCursor = (Screen.lockCursor == false) ? true : false;
    }
}

