using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameController : MonoBehaviour {

    public ModularWorldGenerator dungeonGenerator;
    public KeyPickUpGenerator KeyPickUpGenerator;
    public Text keysCollectedGameText;
    public MovementController moveController;
    public CanvasRenderer endPanel;

    private int keysCollected = 0;
    private int maxKeys = 0;

    private bool playerInEndroom = false;

	// Use this for initialization
	void Start () {
        dungeonGenerator = Instantiate(dungeonGenerator);
        dungeonGenerator.transform.parent = this.transform;
        dungeonGenerator.PrepareGeneration();
        if (LevelProperties.HasChanges()) {
            dungeonGenerator.overridePresetValues(LevelProperties.GetParams());
        }
        dungeonGenerator.GenerateRooms();
        KeyPickUpGenerator = Instantiate(KeyPickUpGenerator);
        
        maxKeys = KeyPickUpGenerator.generatePickUps(dungeonGenerator.getGeneratedModulesWithStartLeadingAndExitLast());
        keysCollectedGameText.text = "x0 / "+maxKeys;

        moveController = Instantiate(moveController);
        endPanel.gameObject.SetActive(false);
    }

    private void Update() {
        //Debug.Log(keysCollected + " / " + maxKeys + " / " + playerInEndroom);
        if (keysCollected == maxKeys && playerInEndroom) {
            EndGame();
        }
    }

    private void EndGame() {
        Debug.Log("Game ended");
        moveController.GameStopped = true;
        endPanel.gameObject.SetActive(true);
    }

    private void PlayerInEndroom(bool inRoom) {
        Debug.Log("Player in endRoom: " + inRoom);
        playerInEndroom = inRoom;

    }

    private void KeyPickedUp() {
        keysCollected++;
        keysCollectedGameText.text = "x"+ keysCollected +" / "+ maxKeys;
    }
}
