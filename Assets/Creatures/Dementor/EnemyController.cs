using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

	public GameObject ThePlayer;
	public float TargetDistance;
	public float AllowedRange = 20;
	public GameObject TheEnemy;
	public float EnemeySpeed;
	public int AttackTrigger;
	public RaycastHit Shot;
	public int HitTrigger;
	public static int Health = 100;

	public int IsAttacking;
	public GameObject ScreenFlash;
	public AudioSource AttackSound;

	void Update(){
		transform.LookAt(ThePlayer.transform);
			if (Physics.Raycast (transform.position, transform.TransformDirection(Vector3.forward), out Shot)){
				TargetDistance = Shot.distance;
			if (TargetDistance < AllowedRange & HitTrigger == 0){
					EnemeySpeed = 0.12f;
				if (AttackTrigger == 0 & HitTrigger == 0){
						TheEnemy.GetComponent<Animation> ().Play("AttackApproachFly");
						transform.position = Vector3.MoveTowards (transform.position,  ThePlayer.transform.position, EnemeySpeed);
					}
				} else{
					EnemeySpeed = 0;
					TheEnemy.GetComponent<Animation> ().Play("Idle");
				}
			}
		if (AttackTrigger == 1 & HitTrigger == 0) {
			if (IsAttacking == 0) {
				StartCoroutine (EnemyDamage ());
				EnemeySpeed = 0;
				TheEnemy.GetComponent<Animation> ().Play ("Attack");}
		}
	}
	void OnCollisionEnter(Collision collision){
		GameObject bullet = collision.gameObject;
		StartCoroutine(HitAnimation());
		Health -= 10;
		if (Health <= 0) {
			Destroy (gameObject);
		}
		Destroy (bullet);
	}
	void OnTriggerEnter(Collider col){
		switch (col.tag) {
		case "Player":
			AttackTrigger = 1;
			break;
		}
	}
	void OnTriggerExit(Collider col){
		switch (col.tag) {
		case "Player":
			AttackTrigger = 0;
			break;
		}
	}
		

	IEnumerator HitAnimation()
	{
		HitTrigger = 1;
		TheEnemy.GetComponent<Animation> ().Play ("Attack");
		yield return new WaitForSeconds (0.2f);
		HitTrigger = 0;
	}

	IEnumerator EnemyDamage(){
		IsAttacking = 1;
		AttackSound.Play ();
		yield return new WaitForSeconds (0.25f);
		ScreenFlash.SetActive (true);
		GlobalHealth.PlayerHealth -= 10;
		yield return new WaitForSeconds (1);
		ScreenFlash.SetActive (false);
		yield return new WaitForSeconds (1.7f);
		IsAttacking = 0;
	}
}
