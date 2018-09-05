
using System.Collections.Generic;
using UnityEngine;

public abstract class ModuleData : UpdatableData {

    [SerializeField]
    protected Module[] modules;

    public List<Module> getModulesFromData() {
        return new List<Module>(modules);
    }

}