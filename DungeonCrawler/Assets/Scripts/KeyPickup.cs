using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickup : MonoBehaviour {

    private void OnTriggerEnter(Collider collider) {
        if (collider.gameObject.tag == "Player") {
            SendMessageUpwards("KeyPickedUp");
            gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate() {
       
        transform.Rotate(2, 2, 0);
    }

}
