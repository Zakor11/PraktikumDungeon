using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "OneWayModules", menuName = "ModuleData/OneWayModuleData", order = 0)]
public class OneWayModuleData : ModuleData {

    protected override void OnValidate() {
        if (modules.Where(e => e.GetExits().Count() != 1).Count() > 0) {
            Debug.LogWarning("There are modules with more than 1 exit in the one-way module data!");
        }
        if (modules.Where(e => e.GetExits().Where(d => !d.hasTag(TileTagsEnum.DeadEnd)).Count() > 1).Count() > 0) {
            Debug.LogWarning("There are exits that don't have a deadend-tag in the one-way module data!");
        }
        base.OnValidate();
    }

}
