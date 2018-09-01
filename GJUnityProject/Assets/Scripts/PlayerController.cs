using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Enums
public enum characterStates { Idle, Flying, AITargeting };

public class PlayerController : MonoBehaviour {

	[Header("References")]
	[SerializeField] Transform charCamera;
	[SerializeField] Image deathFade;
	[SerializeField] GameObject bottledCloud;
	[SerializeField] GameObject deathPanel;
	[SerializeField] GameObject pausePanel;
	[SerializeField] AudioSource music;
	[SerializeField] Slider volumeSlider;
	Rigidbody rb;

	[Header("Inputs")]
	[SerializeField] float mouseSensitivity = 2f;
	[SerializeField] float launchPower = 5f;
	[SerializeField] KeyCode launchKey = KeyCode.Space; // Going to change to unity input later. (???MAYBE???)
	[SerializeField] float minimumVelocity = 0.5f;
	[Header("Collision Modifiers")]
	[SerializeField] float bounceModifier = 1.5f;
	[SerializeField] float launchPadModifier = 2.5f;
	[HideInInspector] public characterStates characterState = characterStates.Idle;
	float pitch = 0;
	float yaw = 0;
	float bottledCloudAmt;
	bool canLand = true;
	bool mouseLocked = true;
	[HideInInspector]
	public bool dying = false;
	bool starting = true;
	
	void Awake(){
		// Cache the rigidbody at awake.
		rb = GetComponent<Rigidbody>();
		deathFade.color = new Color(0, 0, 0, 1);
		Time.timeScale = 1;
		mouseLocked = true;
		deathPanel.SetActive(false);
		volumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1);
		music.volume = volumeSlider.value;
		music.Play();
	}

	void Update () {

		// Do mouse look first.
		if(!dying){
			SimulateMouse();
			LockMouse();
		}else{
			// Make the mouse visible when we die and unlock it.
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		if(Vector3.Distance(transform.position, Vector3.zero) >= 300){
			if(!dying){
				StartCoroutine(Death());
			}
		}

		// Black fade effect
		if(starting){
			deathFade.color = deathFade.color - new Color(0, 0, 0, 1 * Time.deltaTime);
			if(deathFade.color.a <= 0){
				starting = false;
			}
		}

		// Switch our states and call the function for each.
		switch(characterState){
			case(characterStates.Idle):
				IdleInput();
				break;
			case(characterStates.Flying):
				if(bottledCloudAmt >= 1){
					IdleInput();
				}
				// Check if our velocity is less than the minimum wanted.
				if(rb.velocity.magnitude <= minimumVelocity && canLand){
					characterState = characterStates.Idle;
				}
				break;
		}

		// Volume checks
		if(PlayerPrefs.GetFloat("MusicVolume", 1) != volumeSlider.value){
			PlayerPrefs.SetFloat("MusicVolume", volumeSlider.value);
			music.volume = volumeSlider.value;
			PlayerPrefs.Save();
			music.Stop();
			music.Play();
		}
	}

	void LockMouse(){
		if(Input.GetKeyDown(KeyCode.Escape)){
			// If the user pressed escape than unlock/lock the mouse.
			mouseLocked = !mouseLocked;
		}

		if(mouseLocked){
			Time.timeScale = 1;
			pausePanel.SetActive(false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}else{
			Time.timeScale = 0;
			pausePanel.SetActive(true);
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}

	void IdleInput(){
		// When the character is 'idle' (Stuck on a wall) let them launch off of it
		if(Input.GetKeyDown(launchKey)){
			if(characterState == characterStates.Flying){
				bottledCloudAmt --;
				bottledCloud.SetActive(false);
				rb.velocity = Vector3.zero;
			}
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
	}

	IEnumerator landCooldown(){
		// This makes it so when touching a bouncepad or launchpad it wont consider it as ground
		yield return new WaitForSeconds(0.05f);
		canLand = true;
	}

	public IEnumerator Death(){
		dying = true;
		Time.timeScale = 0.1f;
		while(true){
			deathFade.color = deathFade.color + new Color(0, 0, 0, 1 * Time.deltaTime * 5);
			if(deathFade.color.a >= 1){
				//SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				mouseLocked = false;
				deathPanel.SetActive(true);
			}else{
				deathPanel.SetActive(false);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	void OnCollisionEnter(Collision other)
	{
		if(other.gameObject.CompareTag("BottledCloud")){
			bottledCloudAmt = 1;
			bottledCloud.SetActive(true);
			other.gameObject.GetComponent<BoxCollider>().enabled = false;
			other.gameObject.GetComponent<MeshRenderer>().enabled = false;
			rb.AddForce(other.relativeVelocity);
			return;
		}
		// Kill the player if they touch something deadly, aka light.
		if(other.gameObject.CompareTag("Death") && !dying){
			StartCoroutine(Death());
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
			canLand = false;
			StartCoroutine(landCooldown());
			return;
		}else
		// Check if we have collided with a launch pad
		if(other.gameObject.CompareTag("LaunchPad")){
			// Reset our velocity to zero.
			rb.velocity = Vector3.zero;
			// Take the normal of our collision and apply force in that direction.
			rb.AddForce(other.contacts[0].normal * launchPower * launchPadModifier);
			// Dont allow us to stop flying.
			canLand = false;
			StartCoroutine(landCooldown());
			return;
		}else
		// Only remove velocity and set us idle if we are flying
		if(characterState == characterStates.Flying && canLand){
			rb.velocity = Vector3.zero;
			characterState = characterStates.Idle;
		}
	}
	public void Retry(){
		if(deathFade.color.a >= 1){
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		}
	}

	public void BackToMenu(){
		SceneManager.LoadScene(0);
	}

	public void Quit(){
		Application.Quit();
	}

	public void Resume(){
		Time.timeScale = 1;
		mouseLocked = true;
	}
}
