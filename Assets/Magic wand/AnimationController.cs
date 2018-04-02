using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {
	public Animator anim;
	void Start () {
		anim = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
				anim.Play ("Charge");
		}
			if (Input.GetMouseButtonUp (0)) {
				anim.Play ("Cast");
			}
		}
}
