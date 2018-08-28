
using System;
using UnityEngine;

// Very simple smooth mouselook modifier for the MainCamera in Unity
// by Francis R. Griffiths-Keam - www.runningdimensions.com

[AddComponentMenu("Camera/Simple Smooth Mouse Look ")]
public class SimpleSmoothMouseLook : MonoBehaviour
{
    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 180);
    public bool lockCursor;
    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);
    public Vector2 targetDirection;
    public Vector2 targetCharacterDirection;

    [SerializeField]
    Vector3 followDistance = new Vector3(0f, 2f, -10f);
    [SerializeField]
    float distanceDamp = 0.2f;
    
    Transform myT;
    private Transform target;
    Vector3 velocity = Vector3.one;
    public Transform Target
    {
        get {return target; }
        set { target = value; initPosRot(); }
    }

    private void initPosRot()
    {
        myT.position = target.position + followDistance;

        // Set target direction to the camera's initial orientation.
        targetDirection = myT.localRotation.eulerAngles;

        // Set target direction for the character body to its inital state.
        if (target) targetCharacterDirection = target.localRotation.eulerAngles;
    }

    // Use this for initialization
    void Awake()
    {
        myT = transform;
    }
    

    void LateUpdate()
    {
        if (target != null)
        {
            Rotate();
            //Move();
        }
    }

    private void Move()
    {
        Vector3 targetPos = target.position + (target.rotation * followDistance);
        Vector3 curPos = Vector3.SmoothDamp(myT.position, targetPos, ref velocity, distanceDamp);

        myT.position = curPos;
    }

    private void Rotate()
    {
        // Ensure the cursor is always locked when set
        Cursor.lockState = CursorLockMode.Locked;

        // Allow the script to clamp based on a desired target value.
        var targetOrientation = Quaternion.Euler(targetDirection);
        var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        // Find the absolute mouse movement value from point zero.
        _mouseAbsolute += _smoothMouse;

        // Clamp and apply the local x value first, so as not to be affected by world transforms.
        if (clampInDegrees.x < 360)
            _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
        myT.localRotation = xRotation;

        // Then clamp and apply the global y value.
        if (clampInDegrees.y < 360)
            _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        myT.localRotation *= targetOrientation;

        // If there's a character body that acts as a parent to the camera
        //if (target)
        //{
        //    var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, target.up);
        //    target.localRotation = yRotation;
        //    target.localRotation *= targetCharacterOrientation;
        //}
        //else
        //{
            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, myT.InverseTransformDirection(Vector3.up));
            myT.localRotation *= yRotation;
        //}
    }
}