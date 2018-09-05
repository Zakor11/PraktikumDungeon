using UnityEngine;

[CreateAssetMenu()]
public class LevelParams : UpdatableData {

    public int seed = 1337;
    [Range(2, 5)]
    public int maxExits = 2;
    public int mainPathRooms = 10;
    public int maxRoomCount = 20;
    [Range(0, 1)]
    public float endRoomChance = 0.5f;

    protected override void OnValidate() {

        if (maxRoomCount < mainPathRooms) {
            maxRoomCount = mainPathRooms;
        } else if (maxRoomCount > 3 * mainPathRooms) {
            maxRoomCount = 3 * mainPathRooms;
        }
        if (mainPathRooms<1){
            mainPathRooms = 1;
            maxRoomCount = 1;
        }
        base.OnValidate();

    }

}

