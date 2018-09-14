using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerRay : MonoBehaviour
{

    SteamVR_TrackedController _controller;
    LineRenderer lr;


    void OnEnable()
    {
        _controller = GetComponent<SteamVR_TrackedController>();
        _controller.TriggerClicked += TriggerClickedHandler;
        _controller.TriggerUnclicked += TriggerUnclickedHandler;
        Debug.Log("EventHandler added");
    }

    void OnDisable()
    {
        _controller.TriggerClicked -= TriggerClickedHandler;
        _controller.TriggerUnclicked -= TriggerUnclickedHandler;
    }

    private void TriggerClickedHandler(object sender, ClickedEventArgs e)
    {
        SteamVR_Controller.Device mDevice = SteamVR_Controller.Input((int)_controller.GetComponent<SteamVR_TrackedObject>().index);
        float axisValue = mDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x;
        if (axisValue > 0 && axisValue <= 0.8)
        {

            try
            {
                lr = gameObject.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.SetPositions(new Vector3[] { transform.position, transform.position + transform.forward * 400 });
                lr.startColor = Color.red;
                lr.endColor = Color.yellow;
                lr.material.color = Color.cyan;
                lr.SetWidth(0.03f, 0.03f);
                Debug.Log("LineCreated");
            }
            catch (NullReferenceException error)
            {
                Debug.LogWarning(error);
            }

        }
    }

    private void TriggerUnclickedHandler(object sender, ClickedEventArgs e)
    {
        Destroy(_controller.GetComponent<LineRenderer>());
    }

    private void Update()
    {
        SteamVR_Controller.Device mDevice = SteamVR_Controller.Input((int)_controller.GetComponent<SteamVR_TrackedObject>().index);
        Vector2 axisValue = mDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);

        Debug.Log(axisValue);

        if (lr != null)
        {
            lr.SetPositions(new Vector3[] { _controller.transform.position, _controller.transform.position + _controller.transform.forward * 400 });
        }
        RaycastHit hit;
        Physics.Raycast(_controller.transform.position, _controller.transform.forward, out hit, 400);
    }
}
