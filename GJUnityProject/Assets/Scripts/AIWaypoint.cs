using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWaypoint : MonoBehaviour {

	[Header("Pathfinding Variables")]
	[SerializeField] AIWaypoint frontWaypoint;
	[SerializeField] AIWaypoint backWaypoint;
	float editorTime = 0;

	void OnDrawGizmos()
	{
		editorTime += Time.deltaTime;
		// Set the colour and draw a sphere to show the general area of this waypoint.
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, 0.25f);
		// Re-set the colour and draw a line to the front waypoint if its not null.
		Gizmos.color = Color.green;
		if(frontWaypoint != null){
			Gizmos.DrawLine(transform.position, frontWaypoint.transform.position);
			Gizmos.color = Color.cyan;
			// Get the direction towards the front waypoint
			float distance = (frontWaypoint.transform.position - transform.position).magnitude;
			// Get the distance.
			Vector3 direction = (frontWaypoint.transform.position - transform.position) / distance;
			// These cubes are only for editor show, they help show which waypoints are connected and way they can move between them.
			Gizmos.DrawWireCube(new Vector3(transform.position.x + (Mathf.PingPong(editorTime, distance) * direction.x), transform.position.y + (Mathf.PingPong(editorTime, distance) * direction.y), transform.position.z + (Mathf.PingPong(editorTime, distance) * direction.z)), new Vector3(1,1,1));
		}
	}
}
