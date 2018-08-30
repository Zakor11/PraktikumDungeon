using UnityEngine;

public class Module : MonoBehaviour {
    public string[] Tags;
    public ClickableRoomConnection ConnectorPreset;

    public ModuleConnector[] GetExits() {
        return GetComponentsInChildren<ModuleConnector>();
    }
    public PlayerSpawn[] GetSpawns() {
        return GetComponentsInChildren<PlayerSpawn>();
    }

    public void UpdateModuleArrows() {

        foreach (ModuleConnector exit in GetExits()) {
            if (exit.GetComponentInChildren<ClickableRoomConnection>() == null) {
                ClickableRoomConnection connection = (ClickableRoomConnection)Instantiate(ConnectorPreset, exit.transform.position+new Vector3(0,1,0), exit.transform.rotation);
                connection.transform.parent = exit.transform;
            } else {
                var connector = exit.GetComponentInChildren<ClickableRoomConnection>();
                connector.gameObject.SetActive(!connector.gameObject.activeSelf);
            }
        }
    }

}

