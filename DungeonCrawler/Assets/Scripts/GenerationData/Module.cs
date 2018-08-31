using UnityEngine;

[System.Flags]
public enum TileTagsEnum : int {
    None = 0x00,
    Room = 1,
    Corridor = 2,
    Junction = 4,
    DeadEnd = 8,
    Start = 16,
    Path = 32,
    Entrance = 64,
    Outdoor = 128
}


public class Module : MonoBehaviour {

    [SerializeField] [EnumFlagsAttribute]
    public TileTagsEnum tags;

    public ClickableRoomConnection ConnectorPreset;
    
    public ModuleConnector[] GetExits() {
        return GetComponentsInChildren<ModuleConnector>();
    }
    public PlayerSpawn[] GetSpawns() {
        return GetComponentsInChildren<PlayerSpawn>();
    }

    public bool hasTag(TileTagsEnum tag) {
        return (tags & tag) != TileTagsEnum.None;
    }
}

