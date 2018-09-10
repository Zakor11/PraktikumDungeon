using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameController : MonoBehaviour {

    public ModularWorldGenerator dungeonGenerator;
    public KeyPickUpGenerator KeyPickUpGenerator;
    public Text keysCollectedGameText;

    private int keysCollected = 0;
    private int maxKeys = 0;

	// Use this for initialization
	void Start () {
        dungeonGenerator = Instantiate(dungeonGenerator);
        dungeonGenerator.transform.parent = this.transform;
        dungeonGenerator.PrepareGeneration();
        dungeonGenerator.GenerateRooms();
        KeyPickUpGenerator = Instantiate(KeyPickUpGenerator);
        maxKeys = KeyPickUpGenerator.generatePickUps(dungeonGenerator.getGeneratedModulesWithStartLeadingAndExitLast());
        keysCollectedGameText.text = "x0 / "+maxKeys;
	}

    public void KeyPickedUp() {
        keysCollected++;
        keysCollectedGameText.text = "x"+ keysCollected +" / "+ maxKeys;
    }
}
