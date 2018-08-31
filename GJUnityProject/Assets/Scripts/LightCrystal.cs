using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightCrystal : MonoBehaviour {

	public float textUpTimer;

	public bool hasCrystal = false;

	public GameObject crystal;
	public GameObject endPanel;
	public Transform crystalTransform;

	public Text crystalAquiredText;

	void Start () {

	}
	void Update () {
		textUpTimer += Time.fixedDeltaTime;
		crystal.transform.Rotate(Vector3.up * Time.deltaTime * 10);

		if(hasCrystal && textUpTimer <= 2){
			crystalAquiredText.text = "Light crystal aquired";
		}else{
			crystalAquiredText.text = "";
		}

	}
	public void OnCollisionEnter(Collision other){
		if(other.collider.tag == "Crystal"){
			hasCrystal = true;
			textUpTimer = 0f;
			crystal.SetActive(false);
		}
		if(other.collider.tag == "End" && hasCrystal){
			crystalAquiredText.text = "";
			endPanel.SetActive(true);
		}
	}
}
