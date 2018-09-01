using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LightCrystal : MonoBehaviour {

	[SerializeField] GameObject[] enabledWithoutCrystal;
	[SerializeField] GameObject[] enabledWithCrystal;

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

		if(hasCrystal){
			for(int i = 0; i < enabledWithCrystal.Length; i++){
				enabledWithCrystal[i].SetActive(true);
			}
			for(int i = 0; i < enabledWithoutCrystal.Length; i++){
				enabledWithoutCrystal[i].SetActive(false);
			}
		}else{
			for(int i = 0; i < enabledWithCrystal.Length; i++){
				enabledWithCrystal[i].SetActive(false);
			}
			for(int i = 0; i < enabledWithoutCrystal.Length; i++){
				enabledWithoutCrystal[i].SetActive(true);
			}
		}

	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "Crystal"){
			hasCrystal = true;
			textUpTimer = 0f;
			crystal.SetActive(false);
		}
	}

	public void OnCollisionEnter(Collision other){
		if(other.collider.tag == "End" && hasCrystal){
			crystalAquiredText.text = "";
			endPanel.SetActive(true);
			SceneManager.LoadScene(2); // Should be credits scene that gets loaded.
		}
	}
}
