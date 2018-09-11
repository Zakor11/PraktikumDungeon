using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.ThirdPerson;

public class MovementController : MonoBehaviour {

    private AICharacterControl agent;
    public HoveringArrow arrowPrefab;
    private Module currentModule;

    private Vector3 heightOffset = new Vector3(0, 2.5f, 0);
    private bool gameStopped = false;

    public bool GameStopped {
        get {
            return gameStopped;
        }

        set {
            gameStopped = value;
        }
    }

    // Update is called once per frame
    void Update() {
        if (!GameStopped) {
            if (agent == null) {
                agent = GameObject.FindGameObjectWithTag("Player").GetComponent<AICharacterControl>();
            }
            if (agent.getStopstate()) {
                var agentCollider = agent.GetComponent<CapsuleCollider>();
                var possibleCollisions = Physics.OverlapSphere(agentCollider.bounds.center, agentCollider.bounds.extents.magnitude);
                //Debug.Log("Mögliche Playercollisions: "+possibleCollisions.Length);
                foreach (var possibleCollision in possibleCollisions.Where(e => e != agentCollider && e.GetComponentInParent<Module>() != null)) {
                    //Debug.Log("Player auf Mesh: "+possibleCollision.name);
                    currentModule = possibleCollision.GetComponentInParent<Module>();
                    var exits = currentModule.GetExits();
                    foreach (ModuleConnector exit in exits) {
#pragma warning disable CS0618 // Typ oder Element ist veraltet know, but unity is shit
                        exit.gameObject.SetActiveRecursively(true);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                        if (exit.GetComponentInChildren<HoveringArrow>() == null) {
                            HoveringArrow arrow = Instantiate(arrowPrefab, exit.transform.position + heightOffset, Quaternion.LookRotation(-exit.transform.forward));
                            arrow.transform.parent = exit.transform;
                        }
                    }
                }
                handleMovement();
            }
        }
    }

    private void handleMovement() {
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        if (h != 0 || v != 0) {
            var forwardVectorToMatch = currentModule.transform.forward;
            var camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
            var correctiveRotation = Helper.Azimuth(forwardVectorToMatch) - Helper.Azimuth(camForward);
            Debug.Log("RotationTile: " + forwardVectorToMatch + ", Rotation Cam: " + Camera.main.transform.forward + ",Correction: " + correctiveRotation);
            ModuleConnector agentDestinationModuleConnector = null;

            switch (Convert.ToInt32(correctiveRotation)) {
                case 0:
                    Debug.Log("Rotation 0 Case");
                    if (h < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    } else if (h > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    } else if (v < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    } else if (v > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    }
                    break;
                case 90:
                    Debug.Log("Rotation 90 Case");
                    if (h < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    } else if (h > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    } else if (v < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    } else if (v > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    }
                    break;
                case -90:
                    Debug.Log("Rotation -90 Case");
                    if (h < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    } else if (h > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    } else if (v < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    } else if (v > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    }
                    break;
                case 180:
                    Debug.Log("Rotation 180 Case");
                    if (h < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    } else if (h > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    } else if (v < 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    } else if (v > 0) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    }
                    break;
                default:
                    Debug.Log("no matching rotation for movement: " + Convert.ToInt32(correctiveRotation));
                    break;
            }
            if (agentDestinationModuleConnector != null && agentDestinationModuleConnector.getOtherSide() != null) {
                agent.SetTarget(Helper.FindComponentInChildWithTag<Transform>(agentDestinationModuleConnector.getOtherSide().GetComponentInParent<Module>().gameObject, "movePoint").transform);
                disableArrows(currentModule);
            }
        }
    }

    private void disableArrows(Module currentModule) {
        var exits = currentModule.GetExits();
        foreach (ModuleConnector exit in exits) {
            if (exit.GetComponentInChildren<HoveringArrow>().gameObject.activeSelf)
                exit.GetComponentInChildren<HoveringArrow>().gameObject.SetActive(false);
        }
    }
}


