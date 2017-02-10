using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
	CharacterController controller;

	public float walkspeed = 0.01f;


	void Start ()
	{
		controller = GetComponent<CharacterController> ();
	}
	
	void Update ()
	{
		Vector3 direction = new Vector3 (Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		Vector3 movement = Vector3.Normalize(direction) * walkspeed;

		controller.Move (movement);

	}
}
