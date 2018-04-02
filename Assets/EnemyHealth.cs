using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour {
	public int Health = 100;
	public GameObject TheEnemy;
	EnemyController script;


	void Update () {
		if (Health <= 0) {
			script = GetComponent<EnemyController>();
			script.enabled = false;
			TheEnemy.GetComponent<Animation>().Play("Attack");
			EndEnemy();
		}
	}
	IEnumerator EndEnemy () {
		yield return new WaitForSeconds (3);
		Destroy (gameObject);
	}
}
