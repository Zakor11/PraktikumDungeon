
using System.Collections.Generic;
using UnityEngine;

public abstract class ModuleData : UpdatableData {

    [SerializeField]
    private Module[] modules;

    public List<Module> getModulesFromData() {
        return new List<Module>(modules);
    }

}