using UnityEngine;

public class VRMoveableDungeon : MonoBehaviour
{

    [SerializeField]
    private SteamVR_TrackedController controllerToFollow;
    private Transform oldParent;

    public SteamVR_TrackedController ControllerToFollow
    {
        get
        {
            return controllerToFollow;
        }

        set
        {
            if (controllerToFollow != null)
            {
                controllerToFollow.Gripped -= GrippedHandler;
                controllerToFollow.Ungripped -= UngrippedHandler;
            }
            controllerToFollow = value;
            controllerToFollow.Gripped += GrippedHandler;
            controllerToFollow.Ungripped += UngrippedHandler;
        }
    }

    private void OnEnable()
    {
        if (controllerToFollow != null)
        {
            ControllerToFollow.Gripped += GrippedHandler;
            ControllerToFollow.Ungripped += UngrippedHandler;
        }
        oldParent = transform.parent;

    }

    private void OnDisable()
    {
        ControllerToFollow.Gripped -= GrippedHandler;
        ControllerToFollow.Ungripped -= UngrippedHandler;
    }

    private void GrippedHandler(object sender, ClickedEventArgs e)
    {
        this.transform.parent = ControllerToFollow.transform;
    }

    private void UngrippedHandler(object sender, ClickedEventArgs e)
    {
        this.transform.parent = oldParent;
    }


}
