using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogAnimator : MonoBehaviour {
	public Animator anim;
	public int	timer = 0;

	void Start () {
		anim = GetComponent<Animator> ();
	}

	// Update is called once per frame
	void Update () {
		timer += 1;
		if (timer < 500) {
			anim.Play ("Idle");
		}
		else if (timer == 500) {
			anim.Play ("Hop");
		}  
		else if (timer == 540) {
			timer = 0;
		}
	}
}