using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	public static int EnemyHealth = 100;

	void Update () {
	}

	void OnCollisionEnter(Collision collision){
		GameObject enemy = collision.gameObject;
		if(enemy.name == "Dementor"){
			EnemyHealth -= 10;
			if (EnemyHealth <= 0) {
				Destroy (enemy);
			}
		}
		Destroy (gameObject);
	}
}
