using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class ModularWorldGenerator : MonoBehaviour {
    public Module[] Modules;
    public Module StartModule;
    public Module EndModule;
    public GameObject player;
    public int seed = 1337;

    public int MainPathRooms = 50;
    public int MaxRoomCount = 40;
    private int CurrentRooms = 0;
    public float EndRoomChance = 0.5f;


    private int LastCount = -1;
    private int timesSameIteration = 0;

    private List<Module> mainPath = new List<Module>();
    private List<ModuleConnector> pendingExits = new List<ModuleConnector>();

    void Start() {
        
        BuildMainPath();

        CleanUp();

        BuildAdditionalRooms();

        BuildPathEndings();

        mainPath.First().GetComponent<NavMeshSurface>().BuildNavMesh();

        SpawnPlayer();

        mainPath.First().UpdateModuleArrows();
    }

    private void Awake() {
        Random.InitState(seed);
        var startModule = (Module)Instantiate(StartModule, transform.position, transform.rotation);
        startModule.gameObject.name = "Start";
        mainPath.Add(startModule);
        CurrentRooms++;
    }

    //BUILD PATHS
    private void BuildMainPath() {
        MeshCollider endOfPathCollider;
        while (mainPath.Count() < MainPathRooms) {

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
            var mainTag = GetRandom(mainExit.Tags);
            var mainModulePrefab = GetRandomWithTag(Modules.Where(e => e.GetExits().Count() > 1), mainTag);
            var mainModule = (Module)Instantiate(mainModulePrefab);
            mainModule.gameObject.name = CurrentRooms + "";
            var mainModuleExits = mainModule.GetExits();
            var exitToMatch = GetRandom(mainModuleExits);
            MatchExits(mainExit, exitToMatch);

            var mainModuleCollider = mainModule.GetComponent<MeshCollider>();
            endOfPathCollider = mainExit.GetComponentInParent<MeshCollider>();

            if (CollisionDetection(mainModuleCollider, endOfPathCollider)) {
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
                mainModule.GetComponent<MeshRenderer>().material.color = Color.red;
                pendingExits.AddRange(mainExits.Where(e => e.IsMatched() != true));
                CurrentRooms++;
            }

        }

        var finalExits = mainPath.Last().GetExits().Where(e => e.IsMatched() != true).ToArray();
        var finalExit = GetRandom(finalExits);
        //var finalTag = GetRandom(finalExit.Tags);
        var finalModule = (Module)Instantiate(EndModule);
        finalModule.gameObject.name = "Final";
        var finalModuleExits = finalModule.GetExits();
        var finalExitToMatch = finalModuleExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(finalModuleExits);
        MatchExits(finalExit, finalExitToMatch);

        var finalModuleCollider = finalModule.GetComponent<MeshCollider>();
        endOfPathCollider = finalExit.GetComponentInParent<MeshCollider>();

        CurrentRooms++;
        mainPath.Add(finalModule);
        if (CollisionDetection(finalModuleCollider, endOfPathCollider)) {
            Backtrack(3);
            BuildMainPath();
        } else {
            finalExit.SetMatched(true);
            finalExit.setOtherSide(finalExitToMatch);
            finalExitToMatch.SetMatched(true);
            finalExitToMatch.setOtherSide(finalExit);

            pendingExits.AddRange(finalExits.Where(e => e.IsMatched() != true));
            pendingExits.AddRange(finalModuleExits.Where(e => e.IsMatched() != true));
        }

    }

    private void CleanUp() {
        pendingExits = new List<ModuleConnector>(pendingExits.Where(e => e != null));
    }

    private void BuildAdditionalRooms() {

        int save = 0;
        while (CurrentRooms <= MaxRoomCount && pendingExits.Count() > 0 && save < 100) {
            save++;

            var newExit = GetRandom(pendingExits.ToArray());
            var newTag = GetRandom(newExit.Tags);
            var newModulePrefab = GetRandomWithTag(Modules, newTag);
            var newModule = (Module)Instantiate(newModulePrefab);
            newModule.gameObject.name = CurrentRooms + "";
            var newModuleExits = newModule.GetExits();
            var exitToMatch = GetRandom(newModuleExits);
            MatchExits(newExit, exitToMatch);

            var newModuleCollider = newModule.GetComponent<MeshCollider>();
            var endOfPathCollider = newExit.GetComponentInParent<MeshCollider>();

            if (CollisionDetection(newModuleCollider, endOfPathCollider)) {
                //var triedModules = new List<Module>();
                //triedModules.Add(newModule);
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
                pendingExits.AddRange(newModuleExits.Where(e => e.IsMatched() != true));

                CurrentRooms++;
            }

        }
    }

    private void BuildPathEndings() {

        foreach (var pendingExit in pendingExits) {
            if (Random.value < EndRoomChance) {
                //var newTag = GetRandom(pendingExit.Tags);
                var newModulePrefab = GetRandom((Modules.Where(e => e.GetExits().Count() == 1).ToArray()));
                var newModule = (Module)Instantiate(newModulePrefab);
                newModule.gameObject.name = CurrentRooms + "";

                var newModuleExits = newModule.GetExits();
                var exitToMatch = newModuleExits.FirstOrDefault(x => x.IsDefault) ?? GetRandom(newModuleExits);
                MatchExits(pendingExit, exitToMatch);

                var newModuleCollider = newModule.GetComponent<MeshCollider>();
                var endOfPathCollider = pendingExit.GetComponentInParent<MeshCollider>();

                if (CollisionDetection(newModuleCollider, endOfPathCollider)) {
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

    private bool CollisionDetection(MeshCollider newModule, MeshCollider currentModule) {
        var possibleCollisions = Physics.OverlapSphere(newModule.bounds.center, newModule.bounds.extents.magnitude);
        foreach (var possibleCollision in possibleCollisions.Where(e => e != newModule && e != currentModule && e.GetComponent<Module>() != null)) {
            if (newModule != null) {
                Debug.Log("Collision of " + newModule.name + " with " + possibleCollision.gameObject.name + " :" + newModule.bounds.Intersects(possibleCollision.bounds));

                if (newModule.bounds.Intersects(possibleCollision.bounds)) {
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
            var correctiveRotation = Azimuth(forwardVectorToMatch) - Azimuth(newExit.transform.forward);
            newModule.RotateAround(newExit.transform.position, Vector3.up, correctiveRotation);
            var correctiveTranslation = oldExit.transform.position - newExit.transform.position;
            newModule.transform.position += correctiveTranslation;
        } catch (MissingReferenceException e) {
            Debug.Log("Missing Ref catched");
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
        newPlayer.gameObject.AddComponent<MoveToClickPoint>();
       
    }


    // OTHER UTILITY
    private static TItem GetRandom<TItem>(TItem[] array) {
        return array[Random.Range(0, array.Length)];
    }

    private static Module GetRandomWithTag(IEnumerable<Module> modules, string tagToMatch) {
        //Debug.Log("Modules: "+modules.Count());
        var matchingModules = modules.Where(m => m.Tags.Contains(tagToMatch)).ToArray();
        //Debug.Log("Matching Modules for "+tagToMatch+": "+matchingModules.Count());
        return GetRandom(matchingModules);
    }


    private static float Azimuth(Vector3 vector) {
        return Vector3.Angle(Vector3.forward, vector) * Mathf.Sign(vector.x);
    }
}
