using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

public class VRTextField : MonoBehaviour
{

    private InputField field;
    [SerializeField] private VRInput m_VRInput;
    [SerializeField] private VRInteractiveItem m_InteractiveItem;
    private bool m_GazeOver = false;

    private void OnEnable()
    {
        //m_InteractiveItem = GetComponent<VRInteractiveItem>();
        field = GetComponentInParent<InputField>();
        m_VRInput.OnClick += OnClickHandler;
        m_InteractiveItem.OnOver += HandleOver;
        m_InteractiveItem.OnOut += HandleOut;
        //SteamVR.instance.overlay.
    }
    private void OnDisable()
    {
        m_VRInput.OnDown -= OnClickHandler;
        m_InteractiveItem.OnOver -= HandleOver;
        m_InteractiveItem.OnOut -= HandleOut;
        //SteamVR_Events.InputFocus.Listen(OnKeyboard);
    }

    private void OnClickHandler()
    {
        if (m_GazeOver)
        {
            Debug.Log("OnClickHandler");
            field.Select();
            ShowKeyboard();
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
    private void Update()
    {
        //SteamVR.instance.overlay.GetKeyboardText(field.text, 0);
    }

    public void ShowKeyboard()
    {
        SteamVR.instance.overlay.ShowKeyboard(0, 0, "Description", 256, "", true, 0);
        field.text = "";
        
    }

    public void OnKeyboard(object[] args)
    {
        Valve.VR.VREvent_t evt = (Valve.VR.VREvent_t)args[0];
        if (evt.data.keyboard.cNewInput0.ToString() == "\b")
        {
            if (field.text.Length > 0)
            {
                field.text = field.text.Substring(0, field.text.Length - 1);
            }
        }
        else if (evt.data.keyboard.cNewInput0.ToString() == "\x1b")
        {
            SteamVR.instance.overlay.HideKeyboard();
        }
        else
        {
            field.text += evt.data.keyboard.cNewInput0.ToString();
        }
    }

}
