using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour {
	public GameObject bulletPrefab;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonUp(0)){
			GameObject bullet = Instantiate (bulletPrefab, transform.position + transform.forward, Quaternion.identity);
			Rigidbody rigidbody = (Rigidbody)bullet.GetComponent (typeof(Rigidbody));
			rigidbody.AddForce(transform.forward*1000);
		}
	}
}

