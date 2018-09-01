using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottledCloud : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if(other.transform.root.CompareTag("Player")){
			other.transform.root.GetComponent<PlayerController>().GetCloudBottle();
			gameObject.SetActive(false);
		}
	}
}
