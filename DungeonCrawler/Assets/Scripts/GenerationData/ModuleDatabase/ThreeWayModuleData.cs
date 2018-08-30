using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ThreeWayModules", menuName = "ModuleData/ThreeWayModuleData", order = 2)]
public class ThreeWayModuleData : ModuleData {

    protected override void OnValidate() {
        if (this.getModulesFromData().Where(e => e.GetExits().Count() != 3).Count() > 0) {
            Debug.LogWarning("There are exits with other than 3 exits in the three-way module data!");
        }
        base.OnValidate();
    }

}