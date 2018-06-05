using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SapmpleRotScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	int number = 0;
	void Update () {
		number++;
		transform.localRotation = Quaternion.Euler (new Vector3 (0,0,20*Mathf.Sin ((float)number / 10f)));
		if (number == 100)
			number = 0;
	}
}
