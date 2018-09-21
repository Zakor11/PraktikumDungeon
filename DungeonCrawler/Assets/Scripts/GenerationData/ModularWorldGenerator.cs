using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;
using VRStandardAssets.Utils;

public class ModularWorldGenerator : MonoBehaviour
{
    public const TileTagsEnum FALLBACK_TAG = TileTagsEnum.Corridor;
    private const float COLLISION_TOLERANCE_LEVEL = 0.95f;
    public LevelGenData levelGenData;
    public GameObject player;

    private LevelParams genParams;
    private ModuleDatabase Database;
    private Module[] Modules;

    private GameObject moduleHolder;

    private int LastCount = -1;
    private int timesSameIteration = 0;
    private int CurrentRooms = 0;

    private List<Module> mainPath = new List<Module>();
    private List<Module> allModules = new List<Module>();

    private List<ModuleConnector> pendingExits = new List<ModuleConnector>();

    public void PrepareGeneration()
    {

        Database = levelGenData.database;
        genParams = levelGenData.genParams;
        moduleHolder = new GameObject();
        moduleHolder.name = "Module Holder";
        moduleHolder.transform.parent = this.transform.parent;
        moduleHolder.AddComponent<VRInteractiveItem>();
        moduleHolder.AddComponent<VRMoveableDungeon>();
        moduleHolder.GetComponent<VRMoveableDungeon>().ControllerToFollow = GetComponentInParent<GameController>().GrabController;

    }

    public void overridePresetValues(LevelParams levelParams)
    {
        genParams = levelParams;
    }

    public void GenerateRooms()
    {
        Modules = Database.getModulesWithMaxExits(genParams.maxExits);
        UnityEngine.Random.InitState(genParams.seed);
        Module startModule = Instantiate(Helper.GetRandom(Database.getStartRooms()), transform.position, transform.rotation);
        startModule.transform.parent = moduleHolder.transform;
        startModule.gameObject.name = "Start";
        mainPath.Add(startModule);
        CurrentRooms++;

        BuildMainPath();

        CleanUp();

        allModules.AddRange(mainPath);

        BuildAdditionalRooms();

        BuildPathEndings();

        ColorMainPath();

        moduleHolder.AddComponent<NavMeshSurface>();
        NavMeshSurface navBuilder = moduleHolder.GetComponent<NavMeshSurface>();
        navBuilder.collectObjects = CollectObjects.Children;
        navBuilder.overrideVoxelSize = true;
        navBuilder.voxelSize = 0.08f;
        navBuilder.overrideTileSize = true;
        navBuilder.tileSize = 38;
        navBuilder.BuildNavMesh();

        SpawnPlayer();

    }

    private void ColorMainPath()
    {
        foreach (Module mainModule in mainPath)
        {
            mainModule.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        }
    }

    //BUILD PATHS
    private void BuildMainPath()
    {
        while (mainPath.Count() < genParams.mainPathRooms)
        {

            if (LastCount == mainPath.Count())
            {
                timesSameIteration++;
            }
            else
            {
                timesSameIteration = 0;
            }

            if (timesSameIteration >= 5)
            {
                Backtrack(2);

                timesSameIteration = 0;

            }
            LastCount = mainPath.Count();

            ModuleConnector[] mainExits = mainPath.Last().GetExits();
            ModuleConnector mainExitToMatch = Helper.GetRandom(mainExits.Where(e => !e.IsMatched()).ToArray());
            Module newMainModulePrefab = GetRandomMatchingTile(mainExitToMatch, false);
            Module newMainModule = Instantiate(newMainModulePrefab);
            newMainModule.gameObject.name = CurrentRooms + "";
            ModuleConnector newModuleExitToMatch = GetRandomExitWithTag(newMainModule, mainExitToMatch.GetComponentInParent<Module>().tags);
            MatchExits(mainExitToMatch, newModuleExitToMatch);

            if (CollisionDetection(newMainModule, mainExitToMatch.GetComponentInParent<Module>()))
            {
                newMainModule.gameObject.SetActive(false);
                Debug.Log("Gameobject " + newMainModule.name + " disabled");
                Destroy(newMainModule.gameObject);
                newMainModule = null;
            }
            else
            {
                mainExitToMatch.SetMatched(true);
                mainExitToMatch.setOtherSide(newModuleExitToMatch);
                newModuleExitToMatch.SetMatched(true);
                newModuleExitToMatch.setOtherSide(mainExitToMatch);
                mainPath.Add(newMainModule);
                newMainModule.transform.parent = moduleHolder.transform;
                pendingExits.AddRange(mainExits.Where(e => !e.IsMatched()));
                CurrentRooms++;
            }

        }

        Module endModulePrefab = Database.getEndRoom();
        Module endModule = Instantiate(endModulePrefab);
        ModuleConnector finalMainExit = null;
        try
        {
            finalMainExit = GetRandomExitWithTag(mainPath.Last(), endModule.tags);
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.LogWarning(e);
            BuildBridgeForFinal(mainPath.Last());
            finalMainExit = GetRandomExitWithTag(mainPath.Last(), endModule.tags);
        }

        ModuleConnector finalExitToMatch = GetRandomExitWithTag(endModule, finalMainExit.GetComponentInParent<Module>().tags);
        MatchExits(finalMainExit, finalExitToMatch);

        endModule.gameObject.name = "Final";
        CurrentRooms++;
        mainPath.Add(endModule);

        if (CollisionDetection(endModule, finalMainExit.GetComponentInParent<Module>()))
        {
            Backtrack(3);
            BuildMainPath();
        }
        else
        {
            finalMainExit.SetMatched(true);
            finalMainExit.setOtherSide(finalExitToMatch);
            finalExitToMatch.SetMatched(true);
            finalExitToMatch.setOtherSide(finalMainExit);
            endModule.transform.parent = moduleHolder.transform;
            pendingExits.AddRange(finalMainExit.GetComponentInParent<Module>().GetExits().Where(e => e.IsMatched() != true));
            pendingExits.AddRange(endModule.GetExits().Where(e => e.IsMatched() != true));
        }

    }

    private void BuildBridgeForFinal(Module finalMain)
    {

        ModuleConnector finalMainExit = Helper.GetRandom(finalMain.GetExits().Where(e => !e.IsMatched()).ToArray());

        ModuleConnector bridgeConnector = Instantiate(finalMainExit);
        bridgeConnector.name = "BridgeConnector";
        bridgeConnector.transform.parent = finalMainExit.transform.parent;
        bridgeConnector.transform.SetPositionAndRotation(finalMainExit.transform.position, finalMainExit.transform.rotation);
        bridgeConnector.tags = TileTagsEnum.Entrance;

        Module bridgePrefab = GetRandomMatchingTile(bridgeConnector, false);
        Module bridgeModule = (Module)Instantiate(bridgePrefab);
        bridgeModule.gameObject.name = "FinalBridge";
        bridgeConnector.gameObject.SetActive(false);
        ModuleConnector bridgeModuleExitToMatch = GetRandomExitWithTag(bridgeModule, finalMainExit.GetComponentInParent<Module>().tags);
        MatchExits(finalMainExit, bridgeModuleExitToMatch);

        if (CollisionDetection(bridgeModule, finalMainExit.GetComponentInParent<Module>()))
        {
            bridgeModule.gameObject.SetActive(false);
            Debug.Log("Gameobject " + bridgeModule.name + " disabled");
            Destroy(bridgeModule.gameObject);
            bridgeModule = null;

            Destroy(bridgeConnector.gameObject);
            BuildBridgeForFinal(finalMain);
        }
        else
        {
            finalMainExit.SetMatched(true);
            finalMainExit.setOtherSide(bridgeModuleExitToMatch);
            bridgeModuleExitToMatch.SetMatched(true);
            bridgeModuleExitToMatch.setOtherSide(finalMainExit);
            mainPath.Add(bridgeModule);
            bridgeModule.transform.parent = moduleHolder.transform;
            pendingExits.AddRange(finalMain.GetExits().Where(e => !e.IsMatched()));
            CurrentRooms++;
        }
    }

    //Dirty Workaround for null in pending exits
    private void CleanUp()
    {
        int pendingsExitBeforeCleanup = pendingExits.Count();
        pendingExits = pendingExits.Where(e => e != null && !(e.IsMatched() || e.getOtherSide() != null)).ToList();
        if (pendingExits.Count() < pendingsExitBeforeCleanup)
        {
            Debug.LogWarning("Cleanup had to remove exits!");
        }
    }

    private void BuildAdditionalRooms()
    {

        int save = 0;
        while (CurrentRooms < genParams.maxRoomCount && pendingExits.Count() > 0 && save < 100)
        {
            save++;

            ModuleConnector newExit = Helper.GetRandom(pendingExits.ToArray());
            Module newModulePrefab = GetRandomMatchingTile(newExit, false);
            Module newModule = (Module)Instantiate(newModulePrefab);
            newModule.gameObject.name = CurrentRooms + "";
            ModuleConnector exitToMatch = GetRandomExitWithTag(newModule, newExit.GetComponentInParent<Module>().tags);
            MatchExits(newExit, exitToMatch);

            if (CollisionDetection(newModule, newExit.GetComponentInParent<Module>()))
            {
                newModule.gameObject.SetActive(false);
                Debug.Log("Gameobject " + newModule.name + " disabled");
                Destroy(newModule.gameObject);
                newModule = null;
            }

            if (newModule != null)
            {
                newModule.transform.parent = moduleHolder.transform;
                newExit.SetMatched(true);
                newExit.setOtherSide(exitToMatch);
                exitToMatch.SetMatched(true);
                exitToMatch.setOtherSide(newExit);
                pendingExits.Remove(newExit);
                newModule.transform.parent = moduleHolder.transform;
                pendingExits.AddRange(newModule.GetExits().Where(e => e.IsMatched() != true));
                allModules.Add(newModule);
                CurrentRooms++;
            }
            CleanUp();
        }
    }

    private void BuildPathEndings()
    {

        while (pendingExits.Count() > 0)
        {
            ModuleConnector pendingExit = pendingExits.First();
            if (pendingExit.gameObject.activeSelf && pendingExit.transform.parent.gameObject.activeSelf)
            {
                Module newModulePrefab = GetRandomMatchingTile(pendingExit, true);
                Module newModule = (Module)Instantiate(newModulePrefab);
                newModule.gameObject.name = "Endroom " + CurrentRooms;
                ModuleConnector exitToMatch = GetRandomExitWithTag(newModule, pendingExit.GetComponentInParent<Module>().tags);
                MatchExits(pendingExit, exitToMatch);

                EndRoomCollisionHandling(newModule, pendingExit);
            }
            pendingExits = pendingExits.Where(e => (!e.IsMatched() || e.getOtherSide() == null) && e.transform.parent.gameObject.activeSelf).ToList();
        }
    }

    private void EndRoomCollisionHandling(Module newModule, ModuleConnector currentModuleConnector)
    {
        BoxCollider newModuleCollider = newModule.GetComponent<BoxCollider>();
        BoxCollider currentModuleCollider = currentModuleConnector.GetComponentInParent<Module>().GetComponent<BoxCollider>();
        Collider[] possibleCollisions = Physics.OverlapSphere(newModuleCollider.bounds.center, newModuleCollider.bounds.extents.magnitude);
        List<Collider> relevantCollisions = possibleCollisions.Where(e => e != newModuleCollider && e != currentModuleCollider && e.GetComponent<Module>() != null).ToList();
        int intersects = 0;
        foreach (Collider collision in relevantCollisions)
        {
            if (newModuleCollider.bounds.Intersects(collision.bounds))
            {
                intersects++;
                Debug.Log("Relevante Collision für Tile " + newModule.name + ":" + collision.gameObject.name);
            }
        }
        Debug.Log("Relevante Collisions für Anschluss an Tile " + currentModuleConnector.transform.parent.name + ":" + intersects);
        if (intersects > 0)
        {
            float outDistance = 0f;
            //float maxDistance = newModuleCollider.bounds.size.z+currentModuleCollider.bounds.extents.z;
            //Debug.Log("MaxRayDistance for " + currentModuleCollider.name + ": " + maxDistance);
            List<Collider> modulesInExitDirection = relevantCollisions.Where(e => e.bounds.
                  IntersectRay(new Ray(currentModuleCollider.bounds.center, currentModuleConnector.transform.forward), out outDistance)
                  && outDistance <= 6
                  && newModuleCollider.bounds.Intersects(e.bounds))
                  .ToList();

            if (modulesInExitDirection.Count() > 0)
            {

                if (modulesInExitDirection.Count() > 1)
                {
                    Debug.Log("Need to sort raycast intersects");
                    Vector3 currentModuleConnectorPosition = currentModuleConnector.transform.position;

                    modulesInExitDirection.ForEach(e => Debug.Log("Intersected Module: " + e.transform.name +
                        " ; Distance: " + (e.ClosestPoint(currentModuleConnectorPosition) - currentModuleConnectorPosition).magnitude));

                    modulesInExitDirection
                        .Sort((e1, e2) => (e1.ClosestPoint(currentModuleConnectorPosition) - currentModuleConnectorPosition).magnitude
                        .CompareTo((e2.ClosestPoint(currentModuleConnectorPosition) - currentModuleConnectorPosition).magnitude));
                }
                Module adjacentModule = modulesInExitDirection.First().GetComponentInParent<Module>();
                Debug.Log("Colliding Module to work with: " + adjacentModule.name);


                int exits = adjacentModule.GetExits().Where(e => e.IsMatched()).Count();
                newModule.gameObject.SetActive(false);
                Debug.Log("DeadEnd " + newModule.name + " disabled");
                Destroy(newModule.gameObject);
                bool exitsFit = checkIfExitsFitDirectly(currentModuleConnector, adjacentModule);
                bool matched = false;

                Debug.Log("Exits to find: " + (exits + 1));
                if (!exitsFit && adjacentModule.tag != "immutable")
                {
                    matched = FindMatchingModuleWithExits(exits + 1, currentModuleConnector, adjacentModule);
                }
                Debug.Log("Endroommatching: " + (matched | exitsFit));
                if (matched)
                {
                    adjacentModule.gameObject.SetActive(false);
                    //Destroy(adjacentModule.gameObject);
                }
                else
                {
                    Debug.Log("No Match Case");
                    buildDeadendOutOfCurrentRoom(currentModuleConnector);
                    adjacentModule.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.Log("No Frontal Collision");
                newModule.transform.parent = moduleHolder.transform;
                allModules.Add(newModule);
                currentModuleConnector.SetMatched(true);
                ModuleConnector matchedExit = newModule.GetExits().First();
                currentModuleConnector.setOtherSide(matchedExit);
                matchedExit.SetMatched(true);
                matchedExit.setOtherSide(currentModuleConnector);
            }
        }
        else
        {
            Debug.Log("No Collision");
            newModule.transform.parent = moduleHolder.transform;
            allModules.Add(newModule);
            currentModuleConnector.SetMatched(true);
            ModuleConnector matchedExit = newModule.GetExits().First();
            currentModuleConnector.setOtherSide(matchedExit);
            matchedExit.SetMatched(true);
            matchedExit.setOtherSide(currentModuleConnector);
        }
        //currentModuleConnector.gameObject.SetActive(false);
        pendingExits.Remove(currentModuleConnector);
        CurrentRooms++;

    }

    private bool checkIfExitsFitDirectly(ModuleConnector currentModuleConnector, Module collidingModule)
    {
        ModuleConnector[] possibleExits = collidingModule.GetExits();
        foreach (ModuleConnector exit in possibleExits)
        {
            if (exit.transform.position.Equals(currentModuleConnector.transform.position))
            {
                exit.setOtherSide(currentModuleConnector);
                currentModuleConnector.setOtherSide(exit);
                exit.SetMatched(true);
                currentModuleConnector.SetMatched(true);
                return true;
            }
        }
        return false;
    }

    private bool FindMatchingModuleWithExits(int exits, ModuleConnector exitToMatch, Module otherModule)
    {
        List<Collider> colliderList = new List<Collider> {
            exitToMatch.GetComponentInParent<Module>().GetComponent<BoxCollider>()
        };
        foreach (ModuleConnector exit in otherModule.GetExits().Where(e => e.IsMatched() && e.getOtherSide() != null))
        {
            ModuleConnector exitOfModuleToMatch = exit.getOtherSide();
            colliderList.Add(exitOfModuleToMatch.
                GetComponentInParent<Module>().
                GetComponent<BoxCollider>());
        }
        otherModule.gameObject.SetActive(false);
        var possibleModules = Modules.Where(e => e.GetExits().Count() == exits);
        List<ModuleConnector> exitsToMatch = new List<ModuleConnector>();
        exitsToMatch.Add(exitToMatch);
        foreach (ModuleConnector exit in otherModule.GetExits().Where(e => e.IsMatched() && e.getOtherSide() != null))
        {
            exitsToMatch.Add(exit.getOtherSide());
        }
        for (int i = 0; i < possibleModules.Count(); i++)
        {
            int rotations = 0;

            Module testedModulePrefab = possibleModules.ElementAt(i);
            Module testedModule = Instantiate(testedModulePrefab);
            while (rotations < 4)
            {
                Debug.Log("Testing Module " + testedModule.name + " at " + (90 * rotations) + "°");
                List<ModuleConnector> exitsLeftToMatch = exitsToMatch;
                foreach (ModuleConnector testedModuleExit in testedModule.GetExits())
                {

                    exitsLeftToMatch = exitsLeftToMatch.Except(exitsLeftToMatch.Where(e => e.transform.forward == -testedModuleExit.transform.forward &&
                     e.hasTag(testedModule.tags) &&
                      (e.GetComponentInParent<Module>().tags & testedModuleExit.tags) != TileTagsEnum.DeadEnd &&
                      (e.tags & testedModule.tags) != TileTagsEnum.DeadEnd &&
                     testedModuleExit.hasTag(e.GetComponentInParent<Module>().tags))).ToList();

                }
                if (exitsLeftToMatch.Count() > 0)
                {
                    //Debug.Log(exitsLeftToMatch.Count());
                    exitsLeftToMatch = exitsToMatch;
                    testedModule.transform.Rotate(Vector3.up, 90);
                    rotations++;
                }
                else
                {
                    ModuleConnector[] testedModuleExits = testedModule.GetExits();

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
                    allModules.Add(testedModule);
                    if (mainPath.Contains(otherModule))
                    {
                        int index = mainPath.IndexOf(otherModule);
                        mainPath.Insert(index, testedModule);
                    }
                    testedModule.gameObject.name = "Endroom " + CurrentRooms + "(" + otherModule.gameObject.name + ")";
                    testedModule.gameObject.transform.parent = moduleHolder.transform;
                    Debug.Log("Matching suceess: " + testedModule.gameObject.name);
                    return true;
                }
            }
            Destroy(testedModule.gameObject);
        }
        return false;
    }

    private void buildDeadendOutOfCurrentRoom(ModuleConnector currentModuleConnector)
    {
        Module moduleToChange = currentModuleConnector.transform.parent.GetComponent<Module>();
        ModuleConnector[] exitsToMatch = moduleToChange.GetExits().Where(e => e.IsMatched()).ToArray();
        ModuleConnector exitToMatch = Helper.GetRandom<ModuleConnector>(exitsToMatch);
        bool matched = FindMatchingModuleWithExits(exitsToMatch.Count(), exitToMatch.getOtherSide(), moduleToChange);
        if (matched)
        {
            moduleToChange.gameObject.SetActive(false);
            //Destroy(moduleToChange.gameObject);
        }
        else
        {
            Debug.LogError("No DeadendMatch!");
        }
    }

    //PATH UTILITY
    private void Backtrack(int backSteps)
    {
        if (backSteps < mainPath.Count())
        {
            for (int steps = 0; steps < backSteps; steps++)
            {
                Debug.LogWarning("Backsteps to make: " + (backSteps - steps));
                Module moduleToDelete = mainPath.Last();

                ModuleConnector[] exitsToDelete = moduleToDelete.GetExits();
                foreach (ModuleConnector exitToDelete in exitsToDelete)
                {
                    pendingExits.Remove(exitToDelete);
                }

                mainPath.Remove(moduleToDelete);

                var exitsDetached = mainPath.Last().GetExits().Where(e => e.IsMatched() && exitsToDelete.Contains(e.getOtherSide()));

                foreach (ModuleConnector exitdetached in exitsDetached)
                {
                    exitdetached.SetMatched(false);
                    exitdetached.setOtherSide(null);
                }

                moduleToDelete.gameObject.SetActive(false);
                DestroyImmediate(moduleToDelete.gameObject);
                CurrentRooms--;               
            }
        }
        else
        {
            Module startModule = mainPath.First();
            Module moduleToDelete = mainPath.Last();
            while (moduleToDelete != startModule)
            {
                mainPath.Remove(moduleToDelete);
                moduleToDelete.gameObject.SetActive(false);
                Destroy(moduleToDelete.gameObject);
                moduleToDelete = mainPath.Last();
            }

            var exitsDetached = startModule.GetExits().Where(e => e.IsMatched() == true);
            CurrentRooms = 1;
            foreach (ModuleConnector exitdetached in exitsDetached)
            {
                exitdetached.SetMatched(false);
                exitdetached.setOtherSide(null);
            }
            pendingExits.Clear();
        }
    }

    private bool CollisionDetection(Module newModule, Module currentModule)
    {
        BoxCollider newModuleCollider = newModule.GetComponent<BoxCollider>();
        BoxCollider currentModuleCollider = currentModule.GetComponent<BoxCollider>();
        Collider[] possibleCollisions = Physics.OverlapSphere(newModuleCollider.bounds.center, newModuleCollider.bounds.extents.magnitude);
        foreach (Collider possibleCollision in possibleCollisions.Where(e => e != newModuleCollider &&
        e != currentModuleCollider &&
        e.GetComponent<Module>() != null))
        {
            if (newModule != null)
            {
                Bounds newColliderScaledBounds = newModuleCollider.bounds;
                newColliderScaledBounds.extents = newColliderScaledBounds.extents * COLLISION_TOLERANCE_LEVEL;
                Debug.Log("Collision of " + newModule.name + " with " + possibleCollision.name + " :" + newColliderScaledBounds.Intersects(possibleCollision.bounds));

                if (newColliderScaledBounds.Intersects(possibleCollision.bounds))
                {
                    return true;
                }
            }
        }
        return false;
    }


    private void MatchExits(ModuleConnector oldExit, ModuleConnector newExit)
    {
        try
        {
            Transform newModule = newExit.transform.parent;
            Vector3 forwardVectorToMatch = -oldExit.transform.forward;
            float correctiveRotation = Helper.Azimuth(forwardVectorToMatch) - Helper.Azimuth(newExit.transform.forward);
            newModule.RotateAround(newExit.transform.position, Vector3.up, correctiveRotation);
            Vector3 correctiveTranslation = oldExit.transform.position - newExit.transform.position;
            newModule.transform.position += correctiveTranslation;
        }
        catch (MissingReferenceException e)
        {
            Debug.LogError("Missing Ref catched: " + e.Message);
        }
    }

    //PLAYER SPAWN
    private void SpawnPlayer()
    {

        Vector3 spawnPoint = Helper.GetRandom(mainPath.First().GetSpawns()).transform.parent.position;
        Quaternion spawnRotation = Quaternion.identity;
        //Debug.Log("Spawn Point located at: " + spawnPoint.ToString());

        GameObject newPlayer = (GameObject)Instantiate(player, spawnPoint, spawnRotation);
        Debug.Log("Player created");
        Camera.main.GetComponentInParent<FollowCam>().Target = newPlayer.transform;
        newPlayer.gameObject.AddComponent<NavMeshAgent>();
        newPlayer.tag = "Player";
        newPlayer.transform.parent = moduleHolder.transform;
        newPlayer.GetComponent<NefuAIController>().Controller = GetComponentInParent<GameController>().GrabController;
    }


    // OTHER UTILITY
    public Module[] getGeneratedModulesWithStartLeadingAndExitLast()
    {
        List<Module> modules = new List<Module>
        {
            mainPath.First()
        };
        modules.AddRange(allModules.Where(e => e != mainPath.First() && e != mainPath.Last()));
        modules.Add(mainPath.Last());
        return modules.ToArray();
    }

    private static ModuleConnector GetRandomExitWithTag(Module module, TileTagsEnum tagToMatch)
    {
        ModuleConnector[] possibleExits = module.GetExits();
        ModuleConnector[] matchingExits = possibleExits.Where(e => e.hasTag(tagToMatch)).ToArray();
        return Helper.GetRandom(matchingExits);
    }

    private Module GetRandomMatchingTile(ModuleConnector mainExit, bool deadendNeeded)
    {
        //module tags match at least one exittag and have an exit that matches the current module and modules that match deadendneeded
        Debug.Log("MainExit " + mainExit.tag + " of tile " + mainExit.transform.parent.name);
        var possibleModules = Modules.Where(e => e.hasTag(mainExit.tags)
        && (e.tags & mainExit.tags) != TileTagsEnum.DeadEnd
        && e.GetComponentsInChildren<ModuleConnector>().
        Where(d => d.hasTag(mainExit.GetComponentInParent<Module>().tags) && (d.tags & mainExit.GetComponentInParent<Module>().tags) != TileTagsEnum.DeadEnd).Count() > 0
        && e.hasTag(TileTagsEnum.DeadEnd) == deadendNeeded);
        if (possibleModules.Count() > 0)
        {
            return Helper.GetRandom<Module>(possibleModules.ToArray());
        }
        else
        {
            return Helper.GetRandom<Module>(Modules.Where(e => e.hasTag(FALLBACK_TAG) && !e.hasTag(TileTagsEnum.DeadEnd)).ToArray());
        }
    }
}
