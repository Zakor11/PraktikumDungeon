using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.ThirdPerson;

public class MovementController : MonoBehaviour
{

    private NefuAIController agent;
    public HoveringArrow arrowPrefab;
    private Module currentModule;
    private KeywordRecognizer keywordRecognizer;

    private Vector3 heightOffset = new Vector3(0, 2.5f, 0);
    private bool gameStopped = false;
    private bool voiceMovement = false;
    private VoiceDirection voiceDirection = VoiceDirection.NONE;
    Dictionary<string, Action> keywords = new Dictionary<string, Action>();

    public bool GameStopped
    {
        get
        {
            return gameStopped;
        }

        set
        {
            gameStopped = value;
        }
    }

    private enum VoiceDirection : int
    {
        NONE = 0,
        FORWARD = 1,
        BACKWARD = 2,
        RIGHT = 3,
        LEFT = 4
    }

    private void Start()
    {

        keywords.Add("rechts", () =>
        {
            Debug.Log("Voice: rechts");
            voiceMovement = true;
            voiceDirection = VoiceDirection.RIGHT;
        });
        keywords.Add("links", () =>
        {
            Debug.Log("Voice: links");
            voiceMovement = true;
            voiceDirection = VoiceDirection.LEFT;
        });
        keywords.Add("hoch", () =>
        {
            Debug.Log("Voice: hoch");
            voiceMovement = true;
            voiceDirection = VoiceDirection.FORWARD;
        });
        keywords.Add("runter", () =>
        {
            Debug.Log("Voice: runter");
            voiceMovement = true;
            voiceDirection = VoiceDirection.BACKWARD;
        });

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

    }

    void FixedUpdate()
    {
        if (!GameStopped)
        {
            if (agent == null)
            {
                agent = GameObject.FindGameObjectWithTag("Player").GetComponent<NefuAIController>();
            }
            if (agent.getStopstate())
            {
                var agentCollider = agent.GetComponent<CapsuleCollider>();
                var possibleCollisions = Physics.OverlapSphere(agentCollider.bounds.center, agentCollider.bounds.extents.magnitude);
                //Debug.Log("Mögliche Playercollisions: "+possibleCollisions.Length);
                foreach (var possibleCollision in possibleCollisions.Where(e => e != agentCollider && e.GetComponentInParent<Module>() != null))
                {
                    //Debug.Log("Player auf Mesh: "+possibleCollision.name);
                    currentModule = possibleCollision.GetComponentInParent<Module>();
                    var exits = currentModule.GetExits();
                    foreach (ModuleConnector exit in exits)
                    {
#pragma warning disable CS0618 // Typ oder Element ist veraltet know, but unity is shit
                        exit.gameObject.SetActiveRecursively(true);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                        if (exit.GetComponentInChildren<HoveringArrow>() == null)
                        {
                            HoveringArrow arrow = Instantiate(arrowPrefab, exit.transform.position + heightOffset, Quaternion.LookRotation(-exit.transform.forward));
                            arrow.transform.parent = exit.transform;
                        }
                    }
                }
                HandleMovement();
            }
            else
            {
                disableArrows(currentModule);
            }
        }
    }

    private void HandleMovement()
    {

        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        if (h != 0 || v != 0 || voiceMovement)
        {
            Vector3 camUp = Camera.main.transform.up;
            Vector3 camDown = -camUp;

            Vector3 camFront = Camera.main.transform.forward;
            Vector3 camLeft = Quaternion.AngleAxis(-90f,camUp) * camFront;
            Vector3 camRight = Quaternion.AngleAxis(90f, camUp) * camFront;
            Vector3 camBack = -camFront;

            Vector3 currentModuleUp = currentModule.transform.up;
            Vector3 currentModuleCorrection = new Vector3(0, 0, 0);
            var rotDif = Vector3.Angle(camUp, currentModuleUp);

            if (rotDif>45&&rotDif<135 || rotDif>225 && rotDif<315)
            {
                currentModuleCorrection = Quaternion.FromToRotation(currentModuleUp, camUp).eulerAngles;
            }
            Debug.Log("Corrective Rotation: " + currentModuleCorrection);

            Vector3 directionToMatch= new Vector3(0,0,0);
            if (h < 0 ||voiceDirection.Equals(VoiceDirection.LEFT))
            {
                Debug.Log("Case Links");
                directionToMatch = camLeft;
            }
            else if (h > 0 || voiceDirection.Equals(VoiceDirection.RIGHT))
            {
                Debug.Log("Case Rechts");
                directionToMatch = camRight;
            }else if(v<0 || voiceDirection.Equals(VoiceDirection.BACKWARD))
            {
                Debug.Log("Case Runter");
                directionToMatch = camBack;
            }else if(v>0 || voiceDirection.Equals(VoiceDirection.FORWARD))
            {
                Debug.Log("Case Hoch");
                directionToMatch = camFront;
            }
            Debug.Log("Direction to match: " + directionToMatch);
            foreach (ModuleConnector connector in currentModule.GetExits())
            {
                
                Debug.Log("Exit: " + connector.tag + "; Forward: " + Quaternion.Euler(currentModuleCorrection) * connector.transform.forward);
                if (Vector3.Angle(directionToMatch,Quaternion.Euler(currentModuleCorrection)*connector.transform.forward) < 45) {
                    voiceMovement = false;
                    voiceDirection = VoiceDirection.NONE;
                    agent.SetTarget(Helper.FindComponentInChildWithTag<Transform>(connector.getOtherSide().GetComponentInParent<Module>().gameObject, "movePoint").transform);
                    Debug.Log("Matched with "+connector.tag);
                    return;
                }
            }

            voiceMovement = false;
            voiceDirection = VoiceDirection.NONE;

            /*var parentForward = transform.parent.forward;

            var upVectorToMatch = currentModule.transform.up;
            float upCorrection = Helper.Azimuth(upVectorToMatch) - Helper.Azimuth(Vector3.up);

            var forwardVectorToMatch = currentModule.transform.forward;
            if (-45>= upCorrection && upCorrection >= 45) {
               forwardVectorToMatch = Quaternion.Euler(0,0,upCorrection)* forwardVectorToMatch;
            }
            else {
            }

            var camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
            var correctiveRotation = Helper.Azimuth(forwardVectorToMatch) - Helper.Azimuth(camForward);
            Debug.Log("RotationTile: " + forwardVectorToMatch + ", Rotation Cam: " + camForward + ",Correction: " + correctiveRotation);
            ModuleConnector agentDestinationModuleConnector = null;
            int rotation = Convert.ToInt32(correctiveRotation);
            if (-45 <= rotation && rotation <= 45 ||315<rotation)
                rotation = 0;
            else if (-135 <= rotation && rotation < -45 || 225<rotation && rotation<=315)
                rotation = -90;
            else if (45 < rotation && rotation <= 135 || -315<=rotation && rotation<-225)
                rotation = 90;
            else
                rotation = 180;
            switch (rotation) {
                case 0:
                    Debug.Log("Rotation 0 Case");
                    if (h < 0 || voiceDirection == VoiceDirection.LEFT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    } else if (h > 0 || voiceDirection == VoiceDirection.RIGHT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    } else if (v < 0 || voiceDirection == VoiceDirection.BACKWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    } else if (v > 0 || voiceDirection == VoiceDirection.FORWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    }
                    break;
                case 90:
                    Debug.Log("Rotation 90 Case");
                    if (h < 0 || voiceDirection == VoiceDirection.LEFT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    } else if (h > 0 || voiceDirection == VoiceDirection.RIGHT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    } else if (v < 0 || voiceDirection == VoiceDirection.BACKWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    } else if (v > 0 || voiceDirection == VoiceDirection.FORWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    }
                    break;
                case -90:
                    Debug.Log("Rotation -90 Case");
                    if (h < 0 || voiceDirection == VoiceDirection.LEFT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    } else if (h > 0 || voiceDirection == VoiceDirection.RIGHT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    } else if (v < 0 || voiceDirection == VoiceDirection.BACKWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    } else if (v > 0 || voiceDirection == VoiceDirection.FORWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    }
                    break;
                case 180:
                    Debug.Log("Rotation 180 Case");
                    if (h < 0 || voiceDirection == VoiceDirection.LEFT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "right");
                    } else if (h > 0 || voiceDirection == VoiceDirection.RIGHT) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "left");
                    } else if (v < 0 || voiceDirection == VoiceDirection.BACKWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "front");
                    } else if (v > 0 || voiceDirection == VoiceDirection.FORWARD) {
                        agentDestinationModuleConnector = Helper.FindComponentInChildWithTag<ModuleConnector>(currentModule.gameObject, "back");
                    }
                    break;
                default:
                    Debug.Log("no matching rotation for movement: " + Convert.ToInt32(correctiveRotation));
                    break;
            }
            if (agentDestinationModuleConnector != null && agentDestinationModuleConnector.getOtherSide() != null || voiceMovement ) {
                voiceMovement = false;
                voiceDirection = VoiceDirection.NONE;
                agent.SetTarget(Helper.FindComponentInChildWithTag<Transform>(agentDestinationModuleConnector.getOtherSide().GetComponentInParent<Module>().gameObject, "movePoint").transform);           
            }*/
        }
    }

    private void disableArrows(Module currentModule)
    {
        if (currentModule != null)
        {
            var exits = currentModule.GetExits();
            foreach (ModuleConnector exit in exits)
            {
                if (exit.GetComponentInChildren<HoveringArrow>() != null && exit.GetComponentInChildren<HoveringArrow>().gameObject.activeSelf)
                    exit.GetComponentInChildren<HoveringArrow>().gameObject.SetActive(false);
            }
        }
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

}

