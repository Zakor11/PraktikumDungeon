using UnityEngine;

public class ToggleLevelPrefs : MonoBehaviour {

    public void ToggleLevelProperties(bool on) {
        LevelProperties.Changes = false;
    }

    private void Awake() {
        LevelProperties.Changes = false;
    }
}
