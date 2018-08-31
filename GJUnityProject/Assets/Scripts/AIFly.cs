using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Enums
public enum pathDirection { Forward, Backward, None, Player }

public class AIFly : MonoBehaviour {

	// The AI will not pathfind these waypoints (yet?) just because of time constraint.
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
		currentPatrol = patrolPointOne;
		player = GameObject.FindWithTag("Player").transform.root;
	}

	void Update(){
		if(aiState == characterStates.Flying){
			pathDirection direction = GetDirection(currentPatrol, GetClosestWaypoint(transform.position));
			if(direction == pathDirection.None){
				transform.LookAt(currentPatrol.transform);
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
				if(Vector3.Distance(transform.position, currentPatrol.transform.position) <= checkDistance){
					int idleCheck = Random.Range(0, 100);
					if(idleCheck <= idleChance){
						idleTime = Time.time + Random.Range(minIdleTime, maxIdleTime);
					}else{
						idleTime = 0;
					}
					if(currentPatrol == patrolPointOne){
						currentPatrol = patrolPointTwo;
					}else{
						currentPatrol = patrolPointOne;
					}
					aiState = characterStates.Idle;
				}
			}else if(direction == pathDirection.Forward){
				transform.LookAt(GetClosestWaypoint(transform.position).frontWaypoint.transform);
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
			}else if(direction == pathDirection.Backward){
				transform.LookAt(GetClosestWaypoint(transform.position).backWaypoint.transform);
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
			}
		}else if(aiState == characterStates.Idle){
			if(Time.time >= idleTime){
				aiState = characterStates.Flying;
			}
		}else if(aiState == characterStates.AITargeting){
			pathDirection direction = GetDirection(GetClosestWaypoint(player.position), GetClosestWaypoint(transform.position));
			if(direction == pathDirection.None){
				transform.LookAt(player.position);
				transform.position += transform.forward * flyingSpeed * Time.deltaTime;
				if(Vector3.Distance(transform.position, player.position) <= checkDistance){
					// END GAME
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				}
			}else if(direction == pathDirection.Forward){
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

	void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(currentPatrol == null ? Vector3.zero : currentPatrol.transform.position, 0.5f);
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(GetClosestWaypoint(transform.position).transform.position, 0.4f);
	}
}
