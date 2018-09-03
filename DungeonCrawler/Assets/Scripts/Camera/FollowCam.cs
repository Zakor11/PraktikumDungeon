using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour {

    [SerializeField]Vector3 followDistance = new Vector3(0f, 2f, -10f);
    [SerializeField]float distanceDamp = 0.2f;

    public Transform target { get; set; }

    Transform myT;
    Vector3 velocity = Vector3.one;
    
    // Use this for initialization
    void Awake () {
        myT = transform;
	}
	
	// Update is called once per frame
	void LateUpdate () {
        SmoothFollow();
    }

    void SmoothFollow()
    {
        if(target != null)
        {
            Vector3 targetPos = target.position+followDistance; //+ (target.rotation * followDistance);
            Vector3 curPos = Vector3.SmoothDamp(myT.position, targetPos, ref velocity, distanceDamp);

            myT.position = curPos;
            //myT.LookAt(target, target.up);
        }
    }
}
