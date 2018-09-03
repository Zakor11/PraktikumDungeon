using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject {

#if UNITY_EDITOR
    public event System.Action OnValueUpdated;
#endif


    public void NotifyOfUpdates() {

#if UNITY_EDITOR

        UnityEditor.EditorApplication.update -= NotifyOfUpdates;
        if (OnValueUpdated != null) {
            OnValueUpdated();
        }
#endif
    }

    protected virtual void OnValidate() {


#if UNITY_EDITOR

        UnityEditor.EditorApplication.update += NotifyOfUpdates;
#endif



    }
}
