using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ModularWorldGenerator : MonoBehaviour {
    public const TileTagsEnum FALLBACK_TAG = TileTagsEnum.Corridor;

    public LevelGenData levelGenData;
    public GameObject player;
    public MovementController moveControllerPrefab;

    private LevelParams genParams;
    private ModuleDatabase Database;
    private Module[] Modules;

    private int LastCount = -1;
    private int timesSameIteration = 0;
    private int CurrentRooms = 0;

    private List<Module> mainPath = new List<Module>();
    private List<ModuleConnector> pendingExits = new List<ModuleConnector>();

    private void Awake() {
        Database = levelGenData.database;
        genParams = levelGenData.genParams;
        Modules = Database.getModulesWithMaxExits(genParams.maxExits);
        Random.InitState(genParams.seed);
        var startModule = Instantiate(GetRandom(Database.getStartRooms()), transform.position, transform.rotation);
        startModule.gameObject.name = "Start";
        mainPath.Add(startModule);
        CurrentRooms++;
    }

    void Start() {

        BuildMainPath();

        CleanUp();

        BuildAdditionalRooms();

        BuildPathEndings();

        mainPath.First().GetComponent<NavMeshSurface>().BuildNavMesh();

        SpawnPlayer();

        //mainPath.First().UpdateModuleArrows();
    }

    //BUILD PATHS
    private void BuildMainPath() {
        while (mainPath.Count() < genParams.mainPathRooms) {

            if (LastCount == mainPath.Count()) {
                timesSameIteration++;
            } else {
                timesSameIteration = 0;
            }

            if (timesSameIteration >= 5) {
                Backtrack(2);

                timesSameIteration = 0;

            }
            LastCount = mainPath.Count();

            var mainExits = mainPath.Last().GetExits();
            var mainExit = GetRandom(mainPath.Last().GetExits().Where(e => e.IsMatched() != true).ToArray());
            var mainModulePrefab = GetRandomMatchingTile(mainExit,false);
            var mainModule = (Module)Instantiate(mainModulePrefab);
            mainModule.gameObject.name = CurrentRooms + "";
            var exitToMatch = GetRandomExitWithTag(mainModule, mainExit.GetComponentInParent<Module>().tags);
            MatchExits(mainExit, exitToMatch);

            if (CollisionDetection(mainModule, mainExit.GetComponentInParent<Module>())) {
                mainModule.gameObject.SetActive(false);
                Debug.Log("Gameobject " + mainModule.name + " disabled");
                GameObject.DestroyImmediate(mainModule.gameObject);
                mainModule = null;
            }

            if (mainModule != null) {
                mainExit.SetMatched(true);
                mainExit.setOtherSide(exitToMatch);
                exitToMatch.SetMatched(true);
                exitToMatch.setOtherSide(mainExit);
                mainPath.Add(mainModule);
                mainModule.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                pendingExits.AddRange(mainExits.Where(e => e.IsMatched() != true));
                CurrentRooms++;
            }

        }

        var finalExits = mainPath.Last().GetExits().Where(e => e.IsMatched() != true).ToArray();
        var finalExit = GetRandom(finalExits);
        var finalModulePrefab = GetRandomMatchingTile(finalExit, true);
        var finalModule = Instantiate(finalModulePrefab);
        finalModule.gameObject.name = "Final";
        var finalExitToMatch = GetRandomExitWithTag(finalModule, finalExit.GetComponentInParent<Module>().tags);
        MatchExits(finalExit, finalExitToMatch);
        
        CurrentRooms++;
        mainPath.Add(finalModule);
        if (CollisionDetection(finalModule, finalExit.GetComponentInParent<Module>())) {
            Backtrack(3);
            BuildMainPath();
        } else {
            finalExit.SetMatched(true);
            finalExit.setOtherSide(finalExitToMatch);
            finalExitToMatch.SetMatched(true);
            finalExitToMatch.setOtherSide(finalExit);

            pendingExits.AddRange(finalExits.Where(e => e.IsMatched() != true));
            pendingExits.AddRange(finalModule.GetExits().Where(e => e.IsMatched() != true));
        }

    }

    //Dirty Workaround for null in pending exits
    private void CleanUp() {
        pendingExits = new List<ModuleConnector>(pendingExits.Where(e => e != null));
    }

    private void BuildAdditionalRooms() {

        int save = 0;
        while (CurrentRooms < genParams.maxRoomCount && pendingExits.Count() > 0 && save < 100) {
            save++;

            var newExit = GetRandom(pendingExits.ToArray());
            var newModulePrefab = GetRandomMatchingTile(newExit,false);
            var newModule = (Module)Instantiate(newModulePrefab);
            newModule.gameObject.name = CurrentRooms + "";
            var exitToMatch = GetRandomExitWithTag(newModule, newExit.GetComponentInParent<Module>().tags);
            MatchExits(newExit, exitToMatch);

            if (CollisionDetection(newModule, newExit.GetComponentInParent<Module>())) {
                newModule.gameObject.SetActive(false);
                Debug.Log("Gameobject " + newModule.name + " disabled");
                GameObject.Destroy(newModule.gameObject);
                newModule = null;
            }

            if (newModule != null) {
                newExit.SetMatched(true);
                newExit.setOtherSide(exitToMatch);
                exitToMatch.SetMatched(true);
                exitToMatch.setOtherSide(newExit);
                pendingExits.Remove(newExit);
                pendingExits.AddRange(newModule.GetExits().Where(e => e.IsMatched() != true));
                CurrentRooms++;
            }
        }
    }

    private void BuildPathEndings() {

        foreach (var pendingExit in pendingExits) {
            if (Random.value < genParams.endRoomChance) {
                var newModulePrefab = GetRandomMatchingTile(pendingExit,true);
                var newModule = (Module)Instantiate(newModulePrefab);
                newModule.gameObject.name = CurrentRooms + "";

                var newModuleExits = newModule.GetExits();
                var exitToMatch = newModuleExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newModuleExits);
                MatchExits(pendingExit, exitToMatch);

                if (CollisionDetection(newModule, pendingExit.GetComponentInParent<Module>())) {
                    //var triedModules = new List<Module>();
                    //triedModules.Add(newModule);
                    newModule.gameObject.SetActive(false);
                    Debug.Log("Gameobject " + newModule.name + " disabled");
                    GameObject.Destroy(newModule.gameObject);
                    newModule = null;
                }

                if (newModule != null) {
                    pendingExit.SetMatched(true);
                    pendingExit.setOtherSide(exitToMatch);
                    exitToMatch.SetMatched(true);
                    exitToMatch.setOtherSide(pendingExit);

                    CurrentRooms++;
                } else {
                    pendingExit.gameObject.SetActive(false);
                }
            } else {
                pendingExit.gameObject.SetActive(false);
            }
        }

    }


    //PATH UTILITY
    private void Backtrack(int backSteps) {
        if (backSteps < mainPath.Count()) {
            for (int steps = 0; steps < backSteps; steps++) {
                Debug.LogWarning("Backsteps to make: " + (backSteps - steps));
                Debug.LogWarning("Modules in list before deletion: " + mainPath.Count());
                Module moduleToDelete = mainPath.Last();

                var exitsToDelete = moduleToDelete.GetExits();
                foreach (var exitToDelete in exitsToDelete) {
                    if (pendingExits.Contains(exitToDelete)) {
                        pendingExits.Remove(exitToDelete);
                    }
                }

                mainPath.Remove(moduleToDelete);
                moduleToDelete.gameObject.SetActive(false);
                GameObject.DestroyImmediate(moduleToDelete.gameObject);
                Debug.LogWarning("Modules in list after deletion: " + mainPath.Count());
                CurrentRooms--;

            }
            var exitsDetached = mainPath.Last().GetExits().Where(e => e.IsMatched() == true);

            foreach (var exitdetached in exitsDetached) {
                exitdetached.SetMatched(false);
            }

        } else {
            //TODO Delete existing
            var startModule = mainPath.First();
            mainPath = new List<Module> {
                startModule
            };
            var exitsDetached = startModule.GetExits().Where(e => e.IsMatched() == true);

            foreach (var exitdetached in exitsDetached) {
                exitdetached.SetMatched(false);
            }
        }
    }

    private bool CollisionDetection(Module newModule, Module currentModule) {
        var newModuleCollider = newModule.GetComponentInChildren<MeshCollider>();
        var currentModuleCollider = currentModule.GetComponentInChildren<MeshCollider>();
        var possibleCollisions = Physics.OverlapSphere(newModuleCollider.bounds.center, newModuleCollider.bounds.extents.magnitude);
        foreach (var possibleCollision in possibleCollisions.Where(e => e != newModuleCollider && e != currentModuleCollider && e.GetComponentInParent<Module>() != null)) {
            if (newModule != null) {
                Debug.Log("Collision of " + newModule.name + " with " + possibleCollision.gameObject.name + " :" + newModuleCollider.bounds.Intersects(possibleCollision.bounds));

                if (newModuleCollider.bounds.Intersects(possibleCollision.bounds)) {
                    return true;
                }
            }
        }
        return false;
    }


    private void MatchExits(ModuleConnector oldExit, ModuleConnector newExit) {
        try {
            var newModule = newExit.transform.parent;
            var forwardVectorToMatch = -oldExit.transform.forward;
            var correctiveRotation = Helper.Azimuth(forwardVectorToMatch) - Helper.Azimuth(newExit.transform.forward);
            newModule.RotateAround(newExit.transform.position, Vector3.up, correctiveRotation);
            var correctiveTranslation = oldExit.transform.position - newExit.transform.position;
            newModule.transform.position += correctiveTranslation;
        } catch (MissingReferenceException e) {
            Debug.Log("Missing Ref catched: " + e.Message);
        }
    }

    //PLAYER SPAWN
    private void SpawnPlayer() {

        Vector3 spawnPoint = GetRandom(mainPath.First().GetSpawns()).transform.parent.position;
        Quaternion spawnRotation = Quaternion.identity;
        Debug.Log("Spawn Point located at: " + spawnPoint.ToString());

        GameObject newPlayer = (GameObject)Instantiate(player, spawnPoint, spawnRotation);
        Debug.Log("Player created");
        Camera.main.GetComponent<FollowCam>().target = newPlayer.transform;
        newPlayer.gameObject.AddComponent<NavMeshAgent>();
        newPlayer.tag = "Player";
        MovementController movementController = Instantiate(moveControllerPrefab);

    }


    // OTHER UTILITY
    private static TItem GetRandom<TItem>(TItem[] array) {
        return array[Random.Range(0, array.Length)];
    }

    private static ModuleConnector GetRandomExitWithTag(Module module, TileTagsEnum tagToMatch) {
        //Debug.Log("Modules: "+modules.Count());
        var possibleExits = module.GetExits();
        var matchingExits = possibleExits.Where(e => (e.tags & tagToMatch) != TileTagsEnum.None);
        //Debug.Log("Matching Modules for "+tagToMatch+": "+matchingModules.Count());
        return GetRandom<ModuleConnector>(matchingExits.ToArray());
    }

    private Module GetRandomMatchingTile(ModuleConnector mainExit, bool deadendNeeded) {
        //module tags match at least one exittag and have an exit that matches the current module and modules that match deadendneeded
        var possibleModules = Modules.Where(e => e.hasTag(mainExit.tags) &&
        e.GetComponentsInChildren<ModuleConnector>().Where(d => d.hasTag(mainExit.GetComponentInParent<Module>().tags)).Count() > 0 &&
        e.hasTag(TileTagsEnum.DeadEnd)== deadendNeeded);
        if (possibleModules.Count() > 0) {
            return GetRandom<Module>(possibleModules.ToArray());
        } else {
            return GetRandom<Module>(Modules.Where(e => e.hasTag(FALLBACK_TAG) && !e.hasTag(TileTagsEnum.DeadEnd)).ToArray());
        }
    }
}
