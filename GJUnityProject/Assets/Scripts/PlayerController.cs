using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Enums
public enum characterStates { Idle, Flying };

public class PlayerController : MonoBehaviour {

	[Header("References")]
	[SerializeField] Transform charCamera;
	Rigidbody rb;

	[Header("Inputs")]
	[SerializeField] float mouseSensitivity = 2f;
	[SerializeField] float launchPower = 5f;
	[SerializeField] KeyCode launchKey = KeyCode.Space; // Going to change to unity input later.
	[Header("Collision Modifiers")]
	[SerializeField] float bounceModifier = 1.5f;
	[SerializeField] float launchPadModifier = 2.5f;
	[HideInInspector] public characterStates characterState = characterStates.Idle;
	float pitch = 0;
	float yaw = 0;
	bool mouseLocked = false;
	
	void Awake(){
		// Cache the rigidbody at awake.
		rb = GetComponent<Rigidbody>();
	}

	void Update () {
		// Do mouse look first.
		SimulateMouse();
		LockMouse();

		// Switch our states and call the function for each.
		switch(characterState){
			case(characterStates.Idle):
				IdleInput();
				break;
			case(characterStates.Flying):
				// Saved for later use.
				break;
		}
	}

	void LockMouse(){
		// Check if the user pressed the left mouse button in game.
		if(Input.GetKeyDown(KeyCode.Mouse0)){
			// Lock the mouse if they did.
			mouseLocked = true;
		}
		else if(Input.GetKeyDown(KeyCode.Escape)){
			// If the user pressed escape than unlock the mouse.
			mouseLocked = false;
		}

		if(mouseLocked){
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}else{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void IdleInput(){
		// When the character is 'idle' (Stuck on a wall) let them launch off of it
		if(Input.GetKeyDown(launchKey)){
			characterState = characterStates.Flying;
			rb.AddForce(charCamera.forward * launchPower);
		}
	}

	void SimulateMouse(){
		// Mouse look, Limits the up/down to 85 degrees. Only updates the camera transform due to the player transform not needing any rotation updates.
		yaw += (Input.GetAxisRaw("Mouse X") * mouseSensitivity);
		yaw %= 360;

		pitch += (-Input.GetAxisRaw("Mouse Y") * mouseSensitivity);
		pitch = Mathf.Clamp(pitch, -85, 85);

		charCamera.localRotation = Quaternion.Euler(pitch, yaw, 0);

		Vector3 currentRotation = transform.localEulerAngles;
	}

	void OnCollisionEnter(Collision other)
	{
		// Kill the player if they touch something deadly, aka light.
		if(other.gameObject.CompareTag("Death")){
			// TODO: Add a death screen
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
		// Check if we have collided with a bounce pad
		if(other.gameObject.CompareTag("BouncePad")){
			// Reset our velocity to zero.
			rb.velocity = Vector3.zero;
			// Create a new vector3 which takes our relative velocity then inverts the x and z axis so it gives a 'bounce' effect.
			Vector3 newVelocity = new Vector3(-other.relativeVelocity.x, other.relativeVelocity.y, -other.relativeVelocity.z).normalized;
			// Apply the force
			rb.AddForce(newVelocity * launchPower * bounceModifier);
			// Dont allow us to stop flying.
			return;
		}
		// Check if we have collided with a launch pad
		if(other.gameObject.CompareTag("LaunchPad")){
			// Reset our velocity to zero.
			rb.velocity = Vector3.zero;
			// Take the normal of our collision and apply force in that direction.
			rb.AddForce(other.contacts[0].normal * launchPower * launchPadModifier);
			// Dont allow us to stop flying.
			return;
		}
		// Only remove velocity and set us idle if we are flying
		if(characterState == characterStates.Flying){
			rb.velocity = Vector3.zero;
			characterState = characterStates.Idle;
		}
	}
}
