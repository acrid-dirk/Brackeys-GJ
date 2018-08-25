using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

	[SerializeField]GameObject helpPanel;
	[SerializeField]GameObject creditsPanel;
	[SerializeField]GameObject mainPanel;
	GameObject currentPanel;

	
	[SerializeField]Image fadeOut;

	[SerializeField]Color fadeColor = Color.black;

	bool canFade = false;


	void Start () {
		fadeColor.a = 0;
	}
	void Update () {
		if(canFade){
			fadeOut.color = fadeColor;	
			fadeColor.a += 1f * Time.deltaTime;
		}
	}
	public void Play(){
		canFade = true;
	}
	public void Help(){
		currentPanel = helpPanel;
		mainPanel.SetActive(false);
		helpPanel.SetActive(true);
	}
	public void Credits(){
		currentPanel = creditsPanel;
		mainPanel.SetActive(false);
		creditsPanel.SetActive(true);
	}
	public void Back(){
		currentPanel.SetActive(false);
		mainPanel.SetActive(true);
	}
}
