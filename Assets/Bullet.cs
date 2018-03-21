using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	void OnCollisionEnter(Collision collision){
	GameObject enemy = collision.gameObject;
	if(enemy.name == "Target"){
	Destroy (enemy);
	}
	Destroy (gameObject);
	}
	// Update is called once per frame
	void Update () {
		
	}
}
