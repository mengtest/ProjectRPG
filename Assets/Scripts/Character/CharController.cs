using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerMoveType { Walk, Run, Sneak };

public class CharController : MonoBehaviour
{
	CharacterController controller;
	PlayerMoveType playerMoveType = PlayerMoveType.Walk;

	public float walkspeed = 0.07f;
	public float runspeed = 0.12f;
	public float sneakSpeed = 0.03f;

	float moveSpeed;


	void Start ()
	{
		moveSpeed = walkspeed;

		controller = GetComponent<CharacterController> ();
	}
	
	void Update ()
	{
		if (Input.GetButtonDown ("Sneak"))
		{
			if (playerMoveType != PlayerMoveType.Sneak)
			{
				playerMoveType = PlayerMoveType.Sneak;
				moveSpeed = sneakSpeed;
			}
			else if (Input.GetButton ("Run"))
			{
				playerMoveType = PlayerMoveType.Run;
				moveSpeed = runspeed;
			}
			else
			{
				playerMoveType = PlayerMoveType.Walk;
				moveSpeed = walkspeed;
			}
		}
		else if (playerMoveType != PlayerMoveType.Sneak)
		{
			if (Input.GetButton ("Run"))
			{
				playerMoveType = PlayerMoveType.Run;
				moveSpeed = runspeed;
			}
			else
			{
				playerMoveType = PlayerMoveType.Walk;
				moveSpeed = walkspeed;
			}
		}


		Vector3 direction = new Vector3 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		Vector3 movement = Vector3.Normalize(direction) * moveSpeed;

		controller.Move (movement);

	}
}
