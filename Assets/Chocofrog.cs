using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Chocofrog : MonoBehaviour {
	public GameObject Frog;

	void Start () {
		
	}
	
	void Update () {
		Animation();

	}
	IEnumerator Animation(){
		Frog.GetComponent<Animation> ().Play ("C4D Idle");
		yield return new WaitForSeconds (2);
		Frog.GetComponent<Animation> ().Play ("C4D Animation Take");
	}
	void OnTriggerEnter(Collider col){
		switch (col.tag) {
		case "PickUp":
			GlobalScore.Score += 1;
			Destroy (gameObject);
			break;
		}
	}
	}


