using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName ="FourWayModules", menuName = "ModuleData/FourWayModuleData", order =3)]
public class FourWayModuleData : ModuleData {

    protected override void OnValidate() {
        if (this.getModulesFromData().Where(e => e.GetExits().Count() != 4).Count() > 0) {
            Debug.LogWarning("There are exits with other than 4 exits in the four-way module data!");
        }
        base.OnValidate();
    }

}