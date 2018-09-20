using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EndRoomReachedNotifier : MonoBehaviour {

    bool contains = false;

    private void LateUpdate() {
        var collider = GetComponent<BoxCollider>();
        var playerPosition = FindObjectOfType<NavMeshAgent>().transform.position;
        if (collider.bounds.Contains(playerPosition)!=contains){
            contains = !contains;
            SendMessageUpwards("PlayerInEndroom", contains);
        }
    }
}
