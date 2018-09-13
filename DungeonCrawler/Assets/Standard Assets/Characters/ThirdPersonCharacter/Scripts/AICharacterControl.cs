using System;
using UnityEngine;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof (UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof (ThirdPersonCharacter))]
    public class AICharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // the navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // the character we are controlling
        public Transform target;                                    // target to aim for


        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();

	        agent.updateRotation = false;
	        agent.updatePosition = true;
        }


        private void Update()
        {
            if (target != null) {
                agent.SetDestination(target.position);
                //Debug.Log("Destination Set: " + target.position);
            }

            if (agent.remainingDistance > agent.stoppingDistance) {
                agent.isStopped = false;
                character.Move(agent.desiredVelocity, false, false);
                //Debug.Log("Should Move");
            }
            else
            {
                agent.isStopped = true;
                character.Move(Vector3.zero,false,false);
                //Debug.Log("Stop");
            }

        }


        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        public bool getStopstate() {
            if(agent!=null)
                return agent.isStopped;
            else
                return false;
        }
    }
}
