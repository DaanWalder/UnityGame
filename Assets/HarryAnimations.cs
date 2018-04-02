using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarryAnimations : MonoBehaviour {

	public GameObject ThePlayer;

	void Update(){
		if(Input.GetKeyDown("w")){
			ThePlayer.GetComponent<Animation> ().Play("HarryWalk");
				}
		if (Input.GetKeyDown("left shift") & Input.GetKeyDown("w")){
			ThePlayer.GetComponent<Animation> ().Play("HarryRun");
			}
	else{
			ThePlayer.GetComponent<Animation> ().Play ("HarryIdle");
		}
	}
}