using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "TwoWayModules", menuName = "ModuleData/TwoWayModuleData", order = 1)]

public class TwoWayModuleData : ModuleData {

    protected override void OnValidate() {
        if (this.getModulesFromData().Where(e => e.GetExits().Count() != 2).Count() > 0) {
            Debug.LogWarning("There are exits with other than 2 exits in the two-way module data!");
        }
        base.OnValidate();
    }

}