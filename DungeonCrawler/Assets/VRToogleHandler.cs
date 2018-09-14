using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

public class VRToogleHandler : MonoBehaviour {

    private Toggle toggle;
    [SerializeField] private VRInput m_VRInput;
    [SerializeField] private VRInteractiveItem m_InteractiveItem;
    private bool clicked = false;
    private bool m_GazeOver = false;

    private void OnEnable()
    {
        //m_InteractiveItem = GetComponent<VRInteractiveItem>();
        toggle = GetComponentInParent<Toggle>();
        m_VRInput.OnDown += OnClickHandler;
        m_VRInput.OnUp += OnReleaseHandler;
        m_InteractiveItem.OnOver += HandleOver;
        m_InteractiveItem.OnOut += HandleOut;
    }
    private void OnDisable()
    {
        m_VRInput.OnDown -= OnClickHandler;
        m_VRInput.OnUp -= OnReleaseHandler;
        m_InteractiveItem.OnOver -= HandleOver;
        m_InteractiveItem.OnOut -= HandleOut;
    }

    private void OnClickHandler()
    {
        if (!clicked && m_GazeOver) {
            Debug.Log("OnClickHandler");
            toggle.isOn = !toggle.isOn;
            toggle.onValueChanged.Invoke(toggle.isOn);
            clicked = true;
        }
    }

    private void OnReleaseHandler()
    {
        if (clicked)
        {
            clicked = false;
        }
    }

    private void HandleOver()
    {
        Debug.Log("GAAAAZZZEEEE");
        // The user is now looking at the bar.
        m_GazeOver = true;
    }

    private void HandleOut()
    {
        // The user is no longer looking at the bar.
        m_GazeOver = false;
    }
}
