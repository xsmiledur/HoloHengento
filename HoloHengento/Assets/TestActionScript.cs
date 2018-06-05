using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestActionScript : MonoBehaviour {
	Animator UC_Motion;

	void Start () {
		UC_Motion = GetComponent<Animator>();
	}



	// Update is called once per frame
	void Update () {
		AnimatorStateInfo state = UC_Motion.GetCurrentAnimatorStateInfo(0);

		if (Input.GetKeyDown("space")){
			UC_Motion.SetBool("Next", true);
		}

		if (state.IsName ("Base Layer.UMATOBI00")) {
			UC_Motion.SetBool ("Next", false);
		}
	}
}