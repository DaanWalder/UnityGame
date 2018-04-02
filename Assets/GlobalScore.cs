using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalScore : MonoBehaviour {
	public static int Score = 0;
	public GameObject ScoreDisplay;
	public int LocalScore;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		LocalScore = Score;
		ScoreDisplay.GetComponent<Text>().text = Score+ " ";
		if (Score == 3) {
			SceneManager.LoadScene (3);
		}
		
	}
}
