using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyPickUpGenerator : MonoBehaviour {
    private readonly Vector3 PLACEMENT_HEIGHT_OFFSET = new Vector3(1.5f, 0, 0);
    public KeyPickup keyPickUpPrefab;

    public int generatePickUps(Module[] modules) {
        var modulesToConsider = modules.Where(e => (e.hasTag(TileTagsEnum.Room) || e.hasTag(TileTagsEnum.DeadEnd)) && e != modules.First() && e != modules.Last() && e.gameObject.activeSelf).ToArray();
        int keysPlaced = 0;
        if (modulesToConsider.Length > 0) {
            while (keysPlaced < modulesToConsider.Length / 10 +1) {
                var moduleToPlaceKey = Helper.GetRandom<Module>(modulesToConsider);
                if (!moduleHasPickUp(moduleToPlaceKey)) {
                    var transformToPlaceKey = Helper.FindComponentInChildWithTag<Transform>(moduleToPlaceKey.gameObject, "movePoint");
                    var placedKey = Instantiate(keyPickUpPrefab, transformToPlaceKey);
                    placedKey.transform.Translate(PLACEMENT_HEIGHT_OFFSET);
                    Debug.Log("Key placed in Module: " + moduleToPlaceKey.name);
                    placedKey.transform.SetParent(moduleToPlaceKey.transform);
                    keysPlaced++;
                }              
            }
        }
        return keysPlaced;
    }


    private bool moduleHasPickUp(Module module) {
        return module.GetComponentInChildren<KeyPickup>() != null;
    }
}
