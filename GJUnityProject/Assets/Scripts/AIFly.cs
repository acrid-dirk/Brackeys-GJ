using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Enums
public enum pathDirection { Forward, Backward, None, Player }

public class AIFly : MonoBehaviour {

	[SerializeField] AIWaypoint patrolPointOne;
	[SerializeField] AIWaypoint patrolPointTwo;
	[Header("Flying Variables")]
	[SerializeField] float flyingSpeed = 5;
	[SerializeField] float checkDistance = 0.5f;
	[Range(0, 100)]
	[SerializeField] int idleChance = 50; // The % chance to idle when reaching a waypoint, not actually percent based, but close enough.
	[SerializeField] float minIdleTime = 1;
	[SerializeField] float maxIdleTime = 5;

	characterStates aiState = characterStates.Flying;
	AIWaypoint currentPatrol;
	Transform player;
	float idleTime = -1;

	void Start(){
		// Set the default patrol to just the first point.
		currentPatrol = patrolPointOne;
		// Assign the player
		player = GameObject.FindWithTag("Player").transform.root;
	}

	void Update(){
		// Check our current state.
		if(aiState == characterStates.Flying){
			// If we are flying get our direction towards our wanted patrol
			pathDirection direction = GetDirection(currentPatrol, GetClosestWaypoint(transform.position));
			// If we dont have to go to a waypoint, go directly to the waypoint.
			if(direction == pathDirection.None){
				// Look at the waypoint
				transform.LookAt(currentPatrol.transform);
				// Make us move forward at our flying speed.
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
				// Check if we are in the checking distance/
				if(Vector3.Distance(transform.position, currentPatrol.transform.position) <= checkDistance){
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
					// Set us to idle
					aiState = characterStates.Idle;
				}
			}else if(direction == pathDirection.Forward){
				// If we should be moving forward down the waypoints, make us look at the front one
				transform.LookAt(GetClosestWaypoint(transform.position).frontWaypoint.transform);
				// Move us towards the front waypoint
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
			}else if(direction == pathDirection.Backward){
				// We should be moving backwards down the waypoints, same thing as above.
				transform.LookAt(GetClosestWaypoint(transform.position).backWaypoint.transform);
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
			}
		}else if(aiState == characterStates.Idle){
			// Check if were idling. If we are check if we have passed our idle timer.
			if(Time.time >= idleTime){
				aiState = characterStates.Flying;
			}
		}else if(aiState == characterStates.AITargeting){
			// AITargeting is when the player is spotted.
			// Get the waypoint direction.
			pathDirection direction = GetDirection(GetClosestWaypoint(player.position), GetClosestWaypoint(transform.position));
			// If the direction is none then just go directly to player
			if(direction == pathDirection.None){
				// Look at player.
				transform.LookAt(player.position);
				// Go towards them
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
				if(Vector3.Distance(transform.position, player.position) <= checkDistance){
					// If we are within a small radius of the player, 'kill' them
					// END GAME
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				}
			}else if(direction == pathDirection.Forward){
				// We are targetting the player but theres a closer waypoint, so lets move towards that waypoint.
				transform.LookAt(GetClosestWaypoint(transform.position).frontWaypoint.transform);
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
			}else if(direction == pathDirection.Backward){
				transform.LookAt(GetClosestWaypoint(transform.position).backWaypoint.transform);
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
			}
		}
	}

	void TriggerDetection(Collider other)
	{
		// Called when the 'light' sees a player. Changes us to target the player.
		if(other.CompareTag("Player")){
			aiState = characterStates.AITargeting;
		}
	}

	AIWaypoint GetClosestWaypoint(Vector3 pos){
		// Get all the waypoints as an array
		GameObject[] WayPoints = GameObject.FindGameObjectsWithTag("Waypoint");
		// Set the closest range to the max value of float, any high number also works here.
		float closestRange = float.MaxValue;
		// Set our closest point to currently be null because we havent looped through yet.
		AIWaypoint closestPoint = null;
		// Do a for loop to go through all the waypoints.
		for(int i = 0; i < WayPoints.Length; i++){
			// Check if the distance between us if less then the closest range
			if(Vector3.Distance(WayPoints[i].transform.position, pos) <= closestRange){
				// Update the closest range and assign the closest point.
				closestRange = Vector3.Distance(WayPoints[i].transform.position, pos);
				closestPoint = WayPoints[i].GetComponent<AIWaypoint>();
			}
		}
		return closestPoint;
	}

	pathDirection GetDirection(AIWaypoint wantedPoint, AIWaypoint currentPoint){
		int curIndex = 0;
		bool finished = false;
		while(true){
			// Check if the current point is the point we are at, low chances but might happen.
			if(currentPoint == wantedPoint) {
				return pathDirection.None;
			}
			// Forward Check
			AIWaypoint checkedPoint = currentPoint;
			for(int i = 0; i < curIndex; i++){
				if(checkedPoint.frontWaypoint != null){
					checkedPoint = checkedPoint.frontWaypoint;
				}else{
					// We have hit the end, or shouldve if theres no more front waypoints.
					finished = true;
				}
			}
			if(checkedPoint == wantedPoint){
				return pathDirection.Forward;
			}
			curIndex++;

			if(curIndex >= 10000 || finished){
				break;
			}
		}
		// Reset our current Index variable
		finished = false;
		curIndex = 0;
		// Backwards Check.
		while(true){
			// Backward Check
			AIWaypoint checkedPoint = currentPoint;
			for(int i = 0; i < curIndex; i++){
				if(checkedPoint.backWaypoint != null){
					checkedPoint = checkedPoint.backWaypoint;
				}else{
					finished = true;
				}
			}
			if(checkedPoint == wantedPoint){
				return pathDirection.Backward;
			}
			curIndex++;
			if(curIndex >= 10000 || finished){
				break;
			}
		}
		return pathDirection.None;
	}


	// Fancy editor stuff.
	void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(currentPatrol == null ? Vector3.zero : currentPatrol.transform.position, 0.5f);
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(GetClosestWaypoint(transform.position).transform.position, 0.4f);
	}
}
