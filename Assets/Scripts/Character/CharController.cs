using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerMoveType { Walk, Run, Sneak };

public class CharController : MonoBehaviour
{
	CharacterController controller;
	PlayerMoveType playerMoveType = PlayerMoveType.Walk;

	private float health;
	public float startingHealth = 100;
	private float maxHealth;
	public GameObject healthCounter;
	public GameObject healthCounterBackground;

	public float Health
	{
		get
		{
			return health;
		}

		set
		{
			health = Mathf.Clamp (value, 0, maxHealth);
			UpdateHealth ();
		}
	}

	public float MaxHealth
	{
		get
		{
			return maxHealth;
		}

		set
		{
			maxHealth = value;
			UpdateHealth ();
		}
	}


	public float stamina;
	public float startingStamina = 100;
	private float maxStamina;
	public float staminaGenerationPerSecond = 1;
	public GameObject staminaCounter;
	public GameObject staminaCounterBackground;

	public float Stamina
	{
		get
		{
			return stamina;
		}

		set
		{
			stamina = Mathf.Clamp (value, 0, maxStamina);
			UpdateStamina ();
		}
	}

	public float MaxStamina
	{
		get
		{
			return maxStamina;
		}

		set
		{
			maxStamina = value;
			UpdateStamina ();
		}
	}

	public float walkspeed = 0.07f;
	public float runspeed = 0.12f;
	public float staminaLostForRunning = 0.2f;
	public float sneakSpeed = 0.03f;

	//public float timeToAttack = 0.1f;
	//public float attackTime = 0.5f;
	//public float timeAfterAttack = 0.1f;
	//public float weaponDamage = 1;


	float moveSpeed;


	void Start ()
	{
		health = startingHealth;

		stamina = startingStamina;

		if (maxHealth == 0)
		{
			maxHealth = startingHealth;
		}

		if (maxStamina == 0)
		{
			maxStamina = startingStamina;
		}

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
			else if (Input.GetButton ("Run") && Stamina > 0)
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
			if (Input.GetButton ("Run") && Stamina > 0.1f)
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

		if (playerMoveType == PlayerMoveType.Run && movement != Vector3.zero)
		{
			Stamina -= staminaLostForRunning * Time.deltaTime;
		}

		Stamina += staminaGenerationPerSecond * Time.deltaTime;
	}

	void UpdateHealth ()
	{
		healthCounter.GetComponent<RectTransform> ().sizeDelta = new Vector2 
			(healthCounterBackground.GetComponentInParent <RectTransform> ().sizeDelta.x / maxHealth * Health, 
			healthCounterBackground.GetComponent<RectTransform> ().sizeDelta.y);
	}

	void UpdateStamina ()
	{
		staminaCounter.GetComponent<RectTransform> ().sizeDelta = new Vector2
			(staminaCounterBackground.GetComponentInParent<RectTransform> ().sizeDelta.x / maxStamina * Stamina,
			staminaCounterBackground.GetComponent<RectTransform> ().sizeDelta.y);
	}
}
