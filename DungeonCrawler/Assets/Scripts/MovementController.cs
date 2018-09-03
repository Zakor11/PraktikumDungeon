using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.CrossPlatformInput;

public class MovementController : MonoBehaviour {

    private NavMeshAgent agent;
    public HoveringArrow arrowPrefab;
    private Module currentModule;

    private Vector3 heightOffset = new Vector3(0, 1, 0);

    // Update is called once per frame
    void Update() {
        if (agent == null) {
            agent = GameObject.FindGameObjectWithTag("Player").GetComponent<NavMeshAgent>();
        }
        if (agent.isStopped) {
            var agentCollider = agent.GetComponent<CapsuleCollider>();
            var possibleCollisions = Physics.OverlapSphere(agentCollider.bounds.center, agentCollider.bounds.extents.magnitude);
            //Debug.Log("Mögliche Playercollisions: "+possibleCollisions.Length);
            foreach (var possibleCollision in possibleCollisions.Where(e => e != agentCollider && e.GetComponentInParent<Module>() != null)) {
                //Debug.Log("Player auf Mesh: "+possibleCollision.name);
                currentModule = possibleCollision.GetComponentInParent<Module>();
                var exits = currentModule.GetExits();
                foreach (ModuleConnector exit in exits) {
                    if (exit.GetComponentInChildren<HoveringArrow>() == null) {
                        HoveringArrow arrow = Instantiate(arrowPrefab, exit.transform.position + heightOffset, Quaternion.LookRotation(-exit.transform.forward));
                        arrow.transform.parent = exit.transform;
                    }
                    if (!exit.GetComponentInChildren<HoveringArrow>().gameObject.activeSelf)
                        exit.GetComponentInChildren<HoveringArrow>().gameObject.SetActive(true);
                }
            }
            handleMovement();
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

            switch ((int)correctiveRotation) {
                case 0:
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
                    break;
            }
            if (agentDestinationModuleConnector != null) {
                agent.destination = agentDestinationModuleConnector.getOtherSide().GetComponentInParent<Module>().GetComponentInChildren<Renderer>().bounds.center;
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


