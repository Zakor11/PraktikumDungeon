using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelProperties{

    public static bool Changes { get; set; }

    private static int seed;

    public static int GetSeed() {
        return seed;
    }

    public static void SetSeed(string value) {
        seed = int.Parse(value);
    }

    private static int mainPathRooms;

    public static int GetMainPathRooms() {
        return mainPathRooms;
    }

    public static void SetMainPathRooms(string value) {
        mainPathRooms = int.Parse(value);
    }

    private static int maxExits;

    public static int GetMaxExits() {
        return maxExits;
    }

    public static void SetMaxExits(int value) {
        maxExits = value;
    }

    private static int maxRoomCount;

    public static int GetMaxRoomCount() {
        return maxRoomCount;
    }

    public static void SetMaxRoomCount(string value) {
        maxRoomCount = int.Parse(value);
    }

    public static bool HasChanges() {
        return Changes;
    }



    public static LevelParams GetParams() {
        LevelParams genParams = (LevelParams)ScriptableObject.CreateInstance("LevelParams");
        genParams.seed = GetSeed();
        genParams.mainPathRooms = GetMainPathRooms();
        genParams.maxExits = GetMaxExits();
        genParams.maxRoomCount = GetMaxRoomCount();
       
        return genParams;
    }
}
