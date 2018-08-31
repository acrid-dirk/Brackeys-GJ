using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBroadcast : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		SendMessageUpwards("TriggerDetection", other);
	}
}
