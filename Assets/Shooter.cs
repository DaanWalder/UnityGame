using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour {
	public GameObject bulletPrefab;
	public int Casting;
	public float Counter = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0)) {
			Counter += 1;
		}
		if (Counter > 5) {
			if (Input.GetMouseButtonUp (0)) {
				GameObject bullet = Instantiate (bulletPrefab, transform.position + transform.forward, Quaternion.identity);
				Rigidbody rigidbody = (Rigidbody)bullet.GetComponent (typeof(Rigidbody));
				rigidbody.AddForce (transform.forward * 1000);
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			Counter = 0;
		}
	}
}

