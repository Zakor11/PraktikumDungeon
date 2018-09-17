using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class NefuAIController : MonoBehaviour
{

    public NavMeshAgent Agent { get; private set; }             // the navmesh agent required for the path finding
    public ThirdPersonCharacter Character { get; private set; } // the character we are controlling

    public SteamVR_TrackedController Controller
    {
        get
        {
            return controller;
        }

        set
        {
            if (controller != null)
            {
                controller.Gripped -= GrippedHandler;
                controller.Ungripped -= UngrippedHandler;
            }
            controller = value;
            controller.Gripped += GrippedHandler;
            controller.Ungripped += UngrippedHandler;
        }
    }

    public Transform target;                                    // target to aim for
    private SteamVR_TrackedController controller;
    private bool gripped;

    private void Start()
    {
        // get the components on the object we need ( should not be null due to require component so no need to check )
        Agent = GetComponentInChildren<NavMeshAgent>();
        Character = GetComponent<ThirdPersonCharacter>();

        Agent.updateRotation = false;
        Agent.updatePosition = true;
    }

    private void OnEnable()
    {
        if (controller != null)
        {
            Controller.Gripped += GrippedHandler;
            Controller.Ungripped += UngrippedHandler;
        }
    }
    private void OnDisable()
    {
        Controller.Gripped -= GrippedHandler;
        Controller.Ungripped -= UngrippedHandler;
    }

    private void UngrippedHandler(object sender, ClickedEventArgs e)
    {
        gripped = false;
    }

    private void GrippedHandler(object sender, ClickedEventArgs e)
    {
        gripped = true;
    }

    private void Update()
    {
        if (gripped)
        {
            Agent.enabled = false;
        }
        else
        {

            Agent.enabled = true;
            if (target != null)
            {
                Agent.SetDestination(target.position);
                //Debug.Log("Destination Set: " + target.position);
            }

            if (Agent.remainingDistance > Agent.stoppingDistance)
            {
                Agent.isStopped = false;
                Character.Move(Agent.desiredVelocity, false, false);
                //Debug.Log("Should Move");
            }
            else
            {
                Agent.isStopped = true;
                Character.Move(Vector3.zero, false, false);
                //Debug.Log("Stop");
            }
        }

    }


    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public bool getStopstate()
    {
        if (Agent != null)
            return Agent.isStopped;
        else
            return false;
    }
}

