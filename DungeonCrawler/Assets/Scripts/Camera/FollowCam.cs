using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class FollowCam : MonoBehaviour {

    [SerializeField]Vector3 followDistance = new Vector3(0f, 2f, -10f);
    [SerializeField]float distanceDamp = 0.2f;

    public Transform target { get; set; }

    public Material transparentTex;
    public Material normalTex;

    List<MeshRenderer> wallListOld = new List<MeshRenderer>();
    List<MeshRenderer> wallListNew = new List<MeshRenderer>();

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

    void FixedUpdate() {
        RaycastHit hit;

        Debug.DrawLine(transform.position, target.transform.position, Color.white);
        while (Physics.Linecast(Camera.main.transform.position, target.transform.position, out hit, 1<<9, QueryTriggerInteraction.Ignore))
        {
            MeshRenderer wall = hit.collider.GetComponent<MeshRenderer>();
            wallListNew.Add(wall);
            wall.gameObject.layer = 2;
            wall.material = transparentTex;
        }
        Debug.LogError("While verlassen");
        wallListNew.ForEach(e => e.gameObject.layer = 9);
        var toReset = wallListOld.Where(e => !wallListNew.Contains(e)).ToList();
        Debug.Log(toReset.Count());
        Debug.Log(wallListOld.Count());
        toReset.ForEach(e => e.material = normalTex);
        wallListOld.Clear();
        wallListOld.AddRange(wallListNew);
        wallListNew.Clear();
    }
   /* void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, target.transform.position);
    }
    */
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
