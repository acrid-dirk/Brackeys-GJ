using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Enums
public enum AIState { Patrol, Targetting, Idle }

public class AIFly : MonoBehaviour {

	[SerializeField] bool standsStill = false;
	[SerializeField] Transform patrolPointOne;
	[SerializeField] Transform patrolPointTwo;
	[SerializeField] Animator Anim;
	[Header("Flying Variables")]
	[SerializeField] float rotationSpeed = 15f;
	[SerializeField] float flyingSpeed = 5;
	[SerializeField] float checkDistance = 0.5f;
	[Range(0, 100)]
	[SerializeField] int idleChance = 50; // The % chance to idle when reaching a waypoint, not actually percent based, but close enough.
	[SerializeField] float minIdleTime = 1;
	[SerializeField] float maxIdleTime = 5;

	AIState state = AIState.Patrol;
	Transform currentPatrol;
	Transform player;
	float idleTime = -1;

	void Start(){
		// Set the default patrol to just the first point.
		currentPatrol = patrolPointOne;
		// Assign the player
		player = GameObject.FindWithTag("Player").transform.root;
	}

	void Update(){
		// Call the according functions to each state.
		switch(state){
			case(AIState.Idle):
				Idle();
				break;
			case(AIState.Patrol):
				if(!standsStill){
					Patrolling();
				}else{
					state = AIState.Idle;
				}
				break;
			case(AIState.Targetting):
				TargettingPlayer();
				break;
		}
	}

	void TargettingPlayer(){
		// Get the rotation towards the player and do a Spherical Lerp to the rotation.
		Quaternion targetRotation = Quaternion.LookRotation(player.position - transform.position);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		// Go towards them
		transform.position += transform.forward * flyingSpeed * Time.deltaTime;

		// Check if we have killed the player
		if(Vector3.Distance(transform.position, player.position) <= checkDistance){
			// TODO: Cache the Player Controller variable later.
			if(!player.GetComponent<PlayerController>().dying){
				player.GetComponent<PlayerController>().StartCoroutine(player.GetComponent<PlayerController>().Death());
			}
		}

		// Constantly make sure we see the player, if we dont, give up and go back to patrolling
		RaycastHit hit;
		if(Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, Mathf.Infinity)){
			// Check if we hit something other than the player
			if(!hit.transform.root.CompareTag("Player")){
				// Set us to go back to patrolling.
				state = AIState.Patrol;
			}
		}else{
			// If our raycast didnt hit something (Somehow), make us go back to patrolling.
			state = AIState.Patrol;
		}
	}

	void Patrolling(){
		// Get the rotation for the current patrol point
		Quaternion targetRotation = Quaternion.LookRotation(currentPatrol.position - transform.position);
		// Make us lerp to the correct rotation.
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
		// Move towards it
		transform.position += transform.forward * flyingSpeed * Time.deltaTime;

		if(Vector3.Distance(currentPatrol.position, transform.position) <= checkDistance){
			// Do a random.range between 0 and 100 and compare it to our idle chance
			int idleCheck = Random.Range(0, 100);
			if(idleCheck <= idleChance){
				// Set the idle time so we know when to un-idle
				idleTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
			}else{
				// We didnt get idle so just keep flying
				idleTime = 0;
			}

			// Check if our patrol was one of the points
			if(currentPatrol == patrolPointOne){
				currentPatrol = patrolPointTwo;
			}else{
				// If not just set it to first one.
				currentPatrol = patrolPointOne;
			}

			state = AIState.Idle;
		}
	}

	void Idle(){
		if(!standsStill){
			if(Time.time >= idleTime){
				state = AIState.Patrol;
			}
		}else{
			// Constantly make sure we see the player, if we dont, give up and go back to patrolling
			RaycastHit hit;
			if(Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, Mathf.Infinity)){
				// Check if we hit something other than the player
				if(hit.transform.root.CompareTag("Player")){
					// Set us to go back to patrolling.
					state = AIState.Targetting;
				}
			}
		}
	}

	void AlertOthers(){
		// Get all the other AIFlys in the scene
		AIFly[] others = GameObject.FindObjectsOfType(typeof(AIFly)) as AIFly[];
		// For every other AI we are going to call LookForPlayer();
		for(int i = 0; i < others.Length; i++){
			others[i].LookForPlayer();
		}
	}

	// Makes the AI do a raycast for if they can see the player
	public void LookForPlayer(){
		RaycastHit hit;
		if(Physics.Raycast(transform.position, (player.position - transform.position).normalized, out hit, Mathf.Infinity)){
			// If we can see the player lets target them.
			if(hit.transform.root.CompareTag("Player")){
				state = AIState.Targetting;
				Anim.SetTrigger("StartedChase");
			}
		}
	}

	void TriggerDetection(Collider other)
	{
		// Called when the 'light' sees a player. Changes us to target the player.
		if(other.CompareTag("Player")){
			Anim.SetTrigger("StartedChase");
			state = AIState.Targetting;
			AlertOthers();
		}
	}
}
