using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using TMPro;

public class ModularWorldGenerator : MonoBehaviour {
    public const TileTagsEnum FALLBACK_TAG = TileTagsEnum.Corridor;

    public LevelGenData levelGenData;
    public GameObject player;
    public MovementController moveControllerPrefab;

    private LevelParams genParams;
    private ModuleDatabase Database;
    private Module[] Modules;

    private GameObject moduleHolder;

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
        moduleHolder = new GameObject();
        var startModule = Instantiate(GetRandom(Database.getStartRooms()), transform.position, transform.rotation);
        startModule.transform.parent = moduleHolder.transform;
        startModule.gameObject.name = "Start";
        mainPath.Add(startModule);
        CurrentRooms++;
    }

    void Start() {

        BuildMainPath();

        CleanUp();

        BuildAdditionalRooms();

        BuildPathEndings();

        moduleHolder.AddComponent<NavMeshSurface>();
        moduleHolder.GetComponent<NavMeshSurface>().collectObjects = CollectObjects.Children;
        moduleHolder.GetComponent<NavMeshSurface>().BuildNavMesh();

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
            var mainExitToMatch = GetRandom(mainPath.Last().GetExits().Where(e => e.IsMatched() != true).ToArray());
            var newMainModulePrefab = GetRandomMatchingTile(mainExitToMatch, false);
            var newMainModule = (Module)Instantiate(newMainModulePrefab);
            newMainModule.gameObject.name = CurrentRooms + "";
            var newModuleExitToMatch = GetRandomExitWithTag(newMainModule, mainExitToMatch.GetComponentInParent<Module>().tags);
            MatchExits(mainExitToMatch, newModuleExitToMatch);

            if (CollisionDetection(newMainModule, mainExitToMatch.GetComponentInParent<Module>())) {
                newMainModule.gameObject.SetActive(false);
                Debug.Log("Gameobject " + newMainModule.name + " disabled");
                GameObject.DestroyImmediate(newMainModule.gameObject);
                newMainModule = null;
            }
            
            if (newMainModule != null) {
                mainExitToMatch.SetMatched(true);
                mainExitToMatch.setOtherSide(newModuleExitToMatch);
                newModuleExitToMatch.SetMatched(true);
                newModuleExitToMatch.setOtherSide(mainExitToMatch);
                mainPath.Add(newMainModule);
                newMainModule.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                pendingExits.AddRange(mainExits.Where(e => !e.IsMatched()));
                CurrentRooms++;
            }

        }

        var finalMainExits = mainPath.Last().GetExits().Where(e => e.IsMatched() != true).ToArray();
        var finalMainExit = GetRandom(finalMainExits);
        var endModulePrefab = GetRandomMatchingTile(finalMainExit, true);
        var endModule = Instantiate(endModulePrefab);
        endModule.gameObject.name = "Final";
        var finalExitToMatch = GetRandomExitWithTag(endModule, finalMainExit.GetComponentInParent<Module>().tags);
        MatchExits(finalMainExit, finalExitToMatch);

        CurrentRooms++;
        mainPath.Add(endModule);
        if (CollisionDetection(endModule, finalMainExit.GetComponentInParent<Module>())) {
            Backtrack(3);
            BuildMainPath();
        } else {
            finalMainExit.SetMatched(true);
            finalMainExit.setOtherSide(finalExitToMatch);
            finalExitToMatch.SetMatched(true);
            finalExitToMatch.setOtherSide(finalMainExit);
            endModule.transform.parent = moduleHolder.transform;
            pendingExits.AddRange(finalMainExits.Where(e => e.IsMatched() != true));
            pendingExits.AddRange(endModule.GetExits().Where(e => e.IsMatched() != true));
        }

    }

    //Dirty Workaround for null in pending exits
    private void CleanUp() {
        pendingExits = pendingExits.Where(e => e != null && !(e.IsMatched() || e.getOtherSide() != null)).ToList();

    }

    private void BuildAdditionalRooms() {

        int save = 0;
        while (CurrentRooms < genParams.maxRoomCount && pendingExits.Count() > 0 && save < 100) {
            save++;

            var newExit = GetRandom(pendingExits.ToArray());
            var newModulePrefab = GetRandomMatchingTile(newExit, false);
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
                newModule.transform.parent = moduleHolder.transform;
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

        while (pendingExits.Count() > 0) {
            //if (Random.value < genParams.endRoomChance) {
            //    var newModulePrefab = GetRandomMatchingTile(pendingExit,true);
            //    var newModule = (Module)Instantiate(newModulePrefab);
            //    newModule.gameObject.name = CurrentRooms + "";

            //    var newModuleExits = newModule.GetExits();
            //    var exitToMatch = newModuleExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newModuleExits);
            //    MatchExits(pendingExit, exitToMatch);

            //    if (CollisionDetection(newModule, pendingExit.GetComponentInParent<Module>())) {
            //        //var triedModules = new List<Module>();
            //        //triedModules.Add(newModule);
            //        newModule.gameObject.SetActive(false);
            //        Debug.Log("Gameobject " + newModule.name + " disabled");
            //        GameObject.Destroy(newModule.gameObject);
            //        newModule = null;
            //    }

            //    if (newModule != null) {
            //        pendingExit.SetMatched(true);
            //        pendingExit.setOtherSide(exitToMatch);
            //        exitToMatch.SetMatched(true);
            //        exitToMatch.setOtherSide(pendingExit);

            //        CurrentRooms++;
            //    } else {
            //        pendingExit.gameObject.SetActive(false);
            //    }
            //} else {
            //    pendingExit.gameObject.SetActive(false);
            //}
            var pendingExit = pendingExits.First();
            if (pendingExit.gameObject.activeSelf && pendingExit.transform.parent.gameObject.activeSelf) {
                Module newModulePrefab = GetRandomMatchingTile(pendingExit, true);
                var newModule = (Module)Instantiate(newModulePrefab);
                newModule.gameObject.name = "Endroom " + CurrentRooms;

                var newModuleExits = newModule.GetExits();
                ModuleConnector exitToMatch = GetRandomExitWithTag(newModule, pendingExit.GetComponentInParent<Module>().tags);
                MatchExits(pendingExit, exitToMatch);

                EndRoomCollisionHandling(newModule, pendingExit);
            }
            pendingExits = pendingExits.Where(e => (!e.IsMatched() || e.getOtherSide() == null) && e.transform.parent.gameObject.activeSelf).ToList();
        }
    }

    private void EndRoomCollisionHandling(Module newModule, ModuleConnector currentModuleConnector) {
        var newModuleCollider = newModule.GetComponentInChildren<MeshCollider>();
        var currentModuleCollider = currentModuleConnector.GetComponentInParent<Module>().GetComponentInChildren<MeshCollider>();
        var possibleCollisions = Physics.OverlapSphere(newModuleCollider.bounds.center, newModuleCollider.bounds.extents.magnitude);
        var relevantCollisions = possibleCollisions.Where(e => e != newModuleCollider && e != currentModuleCollider && e.GetComponentInParent<Module>() != null);
        int intersects = 0;
        foreach (var collision in relevantCollisions) {
            if (newModuleCollider.bounds.Intersects(collision.bounds)) {
                intersects++;
                Debug.Log("Relevante Collision für Tile " + newModule.name + ":" + collision.transform.parent.gameObject.name);
            }
        }
        Debug.Log("Relevante Collisions für Anschluss an Tile " + currentModuleConnector.transform.parent.name + ":" + intersects);
        if (intersects > 0) {
            var collidingModules = relevantCollisions.Where(e => e.bounds.IntersectRay(new Ray(currentModuleConnector.transform.position, currentModuleConnector.transform.forward))).ToList();
            if (collidingModules.Count() > 0) {

                if (collidingModules.Count() > 1) {
                    Debug.Log("Need to sort raycast intersects");
                    //collidingModules.ForEach(e => Debug.Log("Intersected Module: " + e.transform.parent.name));
                    collidingModules
                        .Sort((e1,e2) => e1.ClosestPoint(currentModuleConnector.transform.position).magnitude.CompareTo(e2.ClosestPoint(currentModuleConnector.transform.position).magnitude));
                }
                Module collidingModule = collidingModules.First().GetComponentInParent<Module>();
                Debug.Log("Colliding Module to work with: " + collidingModule.name);


                int exits = collidingModule.GetExits().Where(e => e.IsMatched()).Count();
                newModule.gameObject.SetActive(false);
                Debug.Log("DeadEnd " + newModule.name + " disabled");
                GameObject.Destroy(newModule.gameObject);
                bool exitsFit = checkIfExitsFitDirectly(currentModuleConnector, collidingModule);
                bool matched = false;

                Debug.Log("Exits to find: " + (exits + 1));
                if (!exitsFit) {
                    matched = FindMatchingModuleWithExits(exits + 1, currentModuleConnector, collidingModule);
                }
                Debug.Log("Endroommatching: " + (matched | exitsFit));
                if (matched) {
                    collidingModule.gameObject.SetActive(false);
                    Destroy(collidingModule);
                } else {
                    //buildDeadendOutOfCurrentRoom();
                    Debug.Log("No Match Case");
                    collidingModule.gameObject.SetActive(true);
                }
            }
        }
        //currentModuleConnector.gameObject.SetActive(false);
        pendingExits.Remove(currentModuleConnector);
        CurrentRooms++;

    }
    private bool checkIfExitsFitDirectly(ModuleConnector currentModuleConnector, Module collidingModule) {
        var possibleExits = collidingModule.GetExits();
        foreach (ModuleConnector exit in possibleExits) {
            if (exit.transform.position.Equals(currentModuleConnector.transform.position)) {
                exit.setOtherSide(currentModuleConnector);
                currentModuleConnector.setOtherSide(exit);
                exit.SetMatched(true);
                currentModuleConnector.SetMatched(true);
                return true;
            }
        }
        return false;
    }

    private bool FindMatchingModuleWithExits(int exits, ModuleConnector exitToMatch, Module otherModule) {
        List<Collider> colliderList = new List<Collider> {
            exitToMatch.GetComponentInParent<Module>().GetComponentInChildren<MeshCollider>()
        };
        foreach (ModuleConnector exit in otherModule.GetExits().Where(e => e.IsMatched() && e.getOtherSide() != null)) {
            var exitOfModuleToMatch = exit.getOtherSide();
            colliderList.Add(exitOfModuleToMatch.
                GetComponentInParent<Module>().
                GetComponentInChildren<MeshCollider>());
        }
        otherModule.gameObject.SetActive(false);
        var possibleModules = Modules.Where(e => e.GetExits().Count() == exits);
        List<ModuleConnector> exitsToMatch = new List<ModuleConnector>();
        exitsToMatch.Add(exitToMatch);
        foreach (ModuleConnector exit in otherModule.GetExits().Where(e => e.IsMatched() && e.getOtherSide() != null)) {
            exitsToMatch.Add(exit.getOtherSide());
        }
        for (int i = 0; i < possibleModules.Count(); i++) {
            int rotations = 0;

            Module testedModulePrefab = possibleModules.ElementAt(i);
            Module testedModule = Instantiate(testedModulePrefab);
            while (rotations < 4) {
                Debug.Log("Testing Module " + testedModule.name + " at " + (90 * rotations) + "°");
                var exitsLeftToMatch = exitsToMatch;
                foreach (ModuleConnector testedModuleExit in testedModule.GetExits()) {

                    exitsLeftToMatch = exitsLeftToMatch.Except(exitsLeftToMatch.Where(e => e.transform.forward == -testedModuleExit.transform.forward &&
                     e.hasTag(testedModule.tags) &&
                     testedModuleExit.hasTag(e.GetComponentInParent<Module>().tags))).ToList();

                }
                if (exitsLeftToMatch.Count() > 0) {
                    //Debug.Log(exitsLeftToMatch.Count());
                    exitsLeftToMatch = exitsToMatch;
                    testedModule.transform.Rotate(Vector3.up, 90);
                    rotations++;
                } else {
                    var testedModuleExits = testedModule.GetExits();

                    testedModule.gameObject.transform.position -=
                        (testedModule.GetExits().First().transform.position -
                        exitsToMatch.Where(e => e.transform.forward == -testedModuleExits.First().transform.forward).First().transform.position);
                    exitsToMatch.ForEach(e => e.SetMatched(true));
                    exitsToMatch.ForEach(e => e.setOtherSide(testedModuleExits.Where(d => -d.transform.forward == e.transform.forward).First()));
                    testedModuleExits.ToList().
                        ForEach(e => e.setOtherSide(
                            exitsToMatch.Where(d => -d.transform.forward == e.transform.forward)
                            .First()));
                    testedModuleExits.ToList().ForEach(e => e.SetMatched(true));

                    testedModule.gameObject.name = "Endroom " + CurrentRooms;
                    Debug.Log("Matching suceess: " + testedModule.gameObject.name);
                    return true;
                }
            }
            Destroy(testedModule.gameObject);
        }
        return false;
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
        var newModuleCollider = newModule.GetComponent<BoxCollider>();
        var currentModuleCollider = currentModule.GetComponent<BoxCollider>();
        var possibleCollisions = Physics.OverlapSphere(newModuleCollider.bounds.center, newModuleCollider.bounds.extents.magnitude);
        foreach (var possibleCollision in possibleCollisions.Where(e => e != newModuleCollider &&
        e != currentModuleCollider &&
        e.GetComponent<Module>() != null)) {
            if (newModule != null) {
                Debug.Log("Collision of " + newModule.name + " with " + possibleCollision.transform.parent.name + " :" + newModuleCollider.bounds.Intersects(possibleCollision.bounds));

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
        var matchingExits = possibleExits.Where(e => (e.hasTag(tagToMatch)));
        //Debug.Log("Matching Modules for "+tagToMatch+": "+matchingModules.Count());
        return GetRandom<ModuleConnector>(matchingExits.ToArray());
    }

    private Module GetRandomMatchingTile(ModuleConnector mainExit, bool deadendNeeded) {
        //module tags match at least one exittag and have an exit that matches the current module and modules that match deadendneeded
        Debug.Log("MainExit " + mainExit.tag + " of tile " + mainExit.transform.parent.name);
        var possibleModules = Modules.Where(e => e.hasTag(mainExit.tags) &&
        e.GetComponentsInChildren<ModuleConnector>().
        Where(d => d.hasTag(mainExit.GetComponentInParent<Module>().tags)).Count() > 0 &&
        e.hasTag(TileTagsEnum.DeadEnd) == deadendNeeded);
        if (possibleModules.Count() > 0) {
            return GetRandom<Module>(possibleModules.ToArray());
        } else {
            return GetRandom<Module>(Modules.Where(e => e.hasTag(FALLBACK_TAG) && !e.hasTag(TileTagsEnum.DeadEnd)).ToArray());
        }
    }
}
