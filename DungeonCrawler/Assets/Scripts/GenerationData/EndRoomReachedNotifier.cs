using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndRoomReachedNotifier : MonoBehaviour {

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            Debug.Log("player entered endroom");
            SendMessageUpwards("PlayerInEndroom", true);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            Debug.Log("Player left endroom");
            SendMessageUpwards("PlayerInEndroom", false);
        }
    }
}
