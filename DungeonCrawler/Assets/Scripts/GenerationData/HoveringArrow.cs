using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoveringArrow : MonoBehaviour {

    private bool upwards=true;
    private int times = 0;
    private int maxTimes = 100;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (upwards) {
            times++;
            transform.position = transform.position + new Vector3(0, 0.005f, 0);
        } else {
            times--;
            transform.position = transform.position - new Vector3(0, 0.005f, 0);
        }
        if (times >= maxTimes || times <= 0) {
            upwards = !upwards;
        }
	}
}
