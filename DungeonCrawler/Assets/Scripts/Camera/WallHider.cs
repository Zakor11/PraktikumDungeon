using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WallHider : MonoBehaviour {

    public Transform Target { get; set; }
    List<MeshRenderer> wallListOld = new List<MeshRenderer>();
    List<MeshRenderer> wallListNew = new List<MeshRenderer>();

    public Material transparentTex;
    public Material normalTex;



    void FixedUpdate()
    {
        if (Target == null) {
            Target = FindObjectOfType<NefuAIController>().transform;
        }
        RaycastHit hit;

        Debug.DrawLine(transform.position, Target.transform.position, Color.white);
        while (Physics.Linecast(Camera.main.transform.position, Target.transform.position, out hit, 1 << 9, QueryTriggerInteraction.Ignore))
        {
            MeshRenderer wall = hit.collider.GetComponent<MeshRenderer>();
            wallListNew.Add(wall);
            wall.gameObject.layer = 2;
            wall.material = transparentTex;
        }
        //Debug.LogError("While verlassen");
        wallListNew.ForEach(e => e.gameObject.layer = 9);
        var toReset = wallListOld.Where(e => !wallListNew.Contains(e)).ToList();
        //Debug.Log(toReset.Count());
        //Debug.Log(wallListOld.Count());
        toReset.ForEach(e => e.material = normalTex);
        wallListOld.Clear();
        wallListOld.AddRange(wallListNew);
        wallListNew.Clear();
    }
}
