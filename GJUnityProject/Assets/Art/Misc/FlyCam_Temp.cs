using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam_Temp : MonoBehaviour {

/*
 * Ported from Windexglow
WASD : basic movement
Shift : Makes camera accelerate
Space : X and Z axis only lock.
*/
	public float mainSpeed = 100f;
	float shiftAdd = 250f; //??
	float maxShift = 1000f; //??
	public float camSens = 0.25f;
	private Vector3 lastMouse;
	private float totalRun;

	void Start ()
	{
		lastMouse = Input.mousePosition;
	}

	void Update () {
		lastMouse = Input.mousePosition - lastMouse ; 
		lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0f ); 
		lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0f); 
		transform.eulerAngles = lastMouse;
		lastMouse = Input.mousePosition;

		//Keyboard commands
		float f;
		Vector3 p = GetBaseInput(); 
		if (Input.GetKeyDown (KeyCode.LeftShift)){ 
			totalRun *= Time.deltaTime;
			p  = p * totalRun * shiftAdd; 
			p.x = Mathf.Clamp(p.x, -maxShift, maxShift); 
			p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
			p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
		}
		else{
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1, 1000); 
			p = p * mainSpeed;
		}

		p = p * Time.deltaTime;

		if (Input.GetKeyUp(KeyCode.Space)){
			f = transform.position.y; 
			transform.Translate(p); 
			transform.position = new Vector3(transform.position.x, f, transform.position.z); 
		}
		else{
			transform.Translate( p); 
		}

	}

	private Vector3 GetBaseInput() {
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey (KeyCode.W)){
			p_Velocity = new Vector3(0, 0 , 1);
		}
		if (Input.GetKey (KeyCode.S)){
			p_Velocity = new Vector3(0, 0 , -1);
		}
		if (Input.GetKey (KeyCode.A)){
			p_Velocity = new Vector3(-1, 0 , 0);
		}
		if (Input.GetKey (KeyCode.D)){
			p_Velocity = new Vector3(1, 0 , 0);
		}
		return p_Velocity;
	}
}
