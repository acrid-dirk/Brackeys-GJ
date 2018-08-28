using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkySphereCuller : MonoBehaviour {

	public float CullDistance = 20f;
	
	// Update is called once per frame
	void Update () {
        float Distance = (Camera.main.transform.position - transform.position).magnitude;
        Vector3 lookAtVector = (transform.position - Camera.main.transform.position).normalized;
        float Dot = Vector3.Dot(lookAtVector, Camera.main.transform.forward);
        print(Dot);
        gameObject.GetComponent<MeshRenderer>().enabled = Distance < CullDistance || Dot > 0;
	}
}
