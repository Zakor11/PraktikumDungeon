using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public ModularWorldGenerator dungeonGenerator;

	// Use this for initialization
	void Start () {
        dungeonGenerator = Instantiate(dungeonGenerator);
        dungeonGenerator.transform.parent = this.transform;
        dungeonGenerator.PrepareGeneration();
        dungeonGenerator.GenerateRooms();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
