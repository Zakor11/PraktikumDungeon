using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ModuleDatabase", menuName = "ModuleDatabase", order = 5)]
public class ModuleDatabase : ScriptableObject {

    public OneWayModuleData oneWays;
    public TwoWayModuleData twoWays;
    public ThreeWayModuleData threeWays;
    public FourWayModuleData fourWays;
    public MultiWayModuleData multiWays;
    private Module StartRoom;
    public Module EndRoom;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public Module[] getModulesWithMaxExits(int maxExits) {
        var modulesList = new List<Module>();

        switch (maxExits) {
            case 5:
                modulesList.AddRange(multiWays.getModulesFromData());
                goto case 4;
            case 4:
                modulesList.AddRange(fourWays.getModulesFromData());
                goto case 3;
            case 3:
                modulesList.AddRange(threeWays.getModulesFromData());
                goto case 2;
            case 2:
                modulesList.AddRange(twoWays.getModulesFromData());
                modulesList.AddRange(oneWays.getModulesFromData());
                break;
            default:
                Debug.LogError("Unknown Identifier for maxExits");
                break;
        }
        initStartAndEnd(maxExits);
        return modulesList.ToArray();
    }

    private void initStartAndEnd(int maxExits) {
        switch (maxExits) {
            case 5:
            case 4:
            case 3:
                StartRoom = threeWays.getModulesFromData().Find(e => e.Tags.Equals("Start"));
                break;
            case 2:
                IEnumerable<Module> Twomodules = twoWays.getModulesFromData();
                StartRoom = Twomodules.Where(e => e.Tags.Contains("Start")).ToArray().First();
                Debug.Log(StartRoom == null);
                break;
            default:
                Debug.LogError("Unknown Identifier for maxExits");
                break;
        }
    }

    public Module getStartRoom() {
        return StartRoom;
    }
    public Module getEndRoom() {
        return EndRoom;
    }
}
