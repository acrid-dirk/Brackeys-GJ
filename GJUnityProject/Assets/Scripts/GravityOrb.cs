using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrb : MonoBehaviour {

	[Header("Gravity Orb Variables")]
	[SerializeField] float effectRadius;
	[SerializeField] float pullModifer = 5f;
	PlayerController PC;

	void Start(){
		// The playercontroller refernce is null by default so lets assign it
		if(PC == null){
			// Find the object in the scene with the 'PlayerController' script. (There should only be one.)
			PC = FindObjectOfType(typeof(PlayerController)) as PlayerController;
		}
	}
	
	void FixedUpdate(){
		// Make sure our player controller is not null
		if(PC != null){
			// Check if we are in the radius and we are flying.
			if(Vector3.Distance(transform.position, PC.transform.position) <= effectRadius && PC.characterState == characterStates.Flying){
				print("PULLING");
				// Get the direction towards the gravity orb (This gameobject) and apply force.
				
				PC.GetComponent<Rigidbody>().AddForce((transform.position - PC.transform.position).normalized * pullModifer * Time.deltaTime);
			}
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, effectRadius);
	}
}
