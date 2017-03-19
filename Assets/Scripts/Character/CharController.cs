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

	// Different speeds and movetype effects
	public float walkspeed = 0.07f;
	public float runspeed = 0.12f;
	public float staminaLostForRunning = 0.2f;
	public float sneakSpeed = 0.03f;
	float moveSpeed;


	// Inventory system
	public List<GameObject> itemSlots;
	public Inventory inventory;
	public RectTransform inventoryBackground;
	Vector3 inventoryBackgroundDestignation;
	public float userInterfaceChangeSpeed = 0.5f;


	public bool hasItemInHand = false;
	public Item itemInHand;
	public GameObject itemInHandGameobject;
	public float itemGrabDistance = 2f;
	public GameObject itemPickupText;

	public Item selectedItem;
	public bool checkedGroundItemsThisFrame = false;
	public bool characterSelectedItem = false;




	void Start ()
	{
		WorldController.currentCharacter = this;
		inventory = new Inventory(itemSlots, OnInventoryOpened, OnInventoryClosed, this);

		inventoryBackground.sizeDelta = new Vector2(inventoryBackground.sizeDelta.x, 10 + Mathf.Ceil(itemSlots.Count/5)*90);
		inventoryBackgroundDestignation = inventoryBackground.localPosition;

		// Create testing items
		WorldController.itemController.CreateItem("Square", 3, inventory, 0);
		WorldController.itemController.CreateItem("Square", 5, inventory, 3);
		WorldController.itemController.CreateItem("Square", 20, inventory, 4);
		WorldController.itemController.CreateItem("Dirt_floor", 25, Vector2.zero);

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
		// Detect movetype
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

		// Calculate movement and move
		Vector3 direction = new Vector3 (Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		Vector3 movement = Vector3.Normalize(direction) * moveSpeed;

		controller.Move (movement);

		if (playerMoveType == PlayerMoveType.Run && movement != Vector3.zero)
		{
			Stamina -= staminaLostForRunning * Time.deltaTime;
		}

		Stamina += staminaGenerationPerSecond * Time.deltaTime;

		// Update the UI
		if(inventoryBackground.localPosition != inventoryBackgroundDestignation)
		{
			inventoryBackground.localPosition = Vector3.Lerp(inventoryBackground.localPosition, inventoryBackgroundDestignation, userInterfaceChangeSpeed * Time.deltaTime);
		}

		if(itemInHandGameobject != null)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Vector3 point = ray.origin + (ray.direction * 1);

			itemInHandGameobject.transform.position = point;
		}
	}

	/// <summary>
	/// Update health graphics
	/// </summary>
	void UpdateHealth ()
	{
		healthCounter.GetComponent<RectTransform> ().sizeDelta = new Vector2 
			(healthCounterBackground.GetComponentInParent <RectTransform> ().sizeDelta.x / maxHealth * Health, 
			healthCounterBackground.GetComponent<RectTransform> ().sizeDelta.y);
	}

	/// <summary>
	/// Update stamina graphics
	/// </summary>
	void UpdateStamina ()
	{
		staminaCounter.GetComponent<RectTransform> ().sizeDelta = new Vector2
			(staminaCounterBackground.GetComponentInParent<RectTransform> ().sizeDelta.x / maxStamina * Stamina,
			staminaCounterBackground.GetComponent<RectTransform> ().sizeDelta.y);
	}

	void OnInventoryOpened(Inventory inventory)
	{
		inventory.isOpened = true;
		inventoryBackgroundDestignation = new Vector3(inventoryBackground.localPosition.x, inventoryBackground.localPosition.y - 95 + inventoryBackground.sizeDelta.y);
	}
	
	void OnInventoryClosed(Inventory inventory)
	{
		inventory.isOpened = false;
		inventoryBackgroundDestignation = new Vector3(inventoryBackground.localPosition.x, inventoryBackground.localPosition.y + 95 - inventoryBackground.sizeDelta.y);
	}

	public void ChangeInventoryOpenStatus()
	{
		if(inventory.isOpened == false)
		{
			inventory.onOpened(inventory);
		}
		else
		{
			inventory.onClosed(inventory);
		}
	}

	/// <summary>
	/// Picks an item from the itemSlot and puts it into itemInHand.
	/// </summary>
	/// <param name="itemSlot">The itemSlot where to kick the item.</param>
	/// <param name="amount">The amount to pick from the itemSlot i</param>
	public void PickItemFromItemSlot(GameObject itemSlot, int amount = -1)
	{
		Inventory inventory = WorldController.itemController.itemSlotInventories [itemSlot];

		if (itemSlot == null || inventory == null)
		{
			Debug.Log("Can't pick item from null itemSlot or inventory.");
			return;
		}

		Item item = inventory.GetItemFromItemSlot(itemSlot, amount);

		if (item == null)
		{
			Debug.Log ("Trying to pick an item which doesn't exist.");
			return;
		}

		if(itemInHand == null)
		{
			itemInHand = item;
			itemInHandGameobject = item.itemObject;
			itemInHandGameobject.transform.SetParent(WorldController.itemController.inventory);
		}
		else if(itemInHand.itemName == item.itemName)
		{
			if(itemInHand.maxStackSize - itemInHand.StackSize >= item.StackSize)
			{
				itemInHand.Add(item.StackSize);
			}
			else
			{
				Debug.Log("Trying to pick too much items from itemSlot. Returning the amount which couldn't be picked up.");
				itemInHand.Add(itemInHand.maxStackSize - item.StackSize);

				item.Remove(itemInHand.maxStackSize - item.StackSize);
				inventory.AddItem(item, inventory.GetItemSlotID(itemSlot));
				
			}
		}
		else
		{
			Debug.Log("Trying to pick an item from an itemSlot when already holding an item and it isn't the same type. Returning it.");
			inventory.AddItem(item, inventory.GetItemSlotID(itemSlot));
		}

		inventory.UpdateItemSlots ();
		UpdateItemInHand ();		
	}

	/// <summary>
	/// Puts the item in hand to the itemSlot.
	/// </summary>
	/// <param name="itemSlot">The itemSlot where to put the item in hand.</param>
	/// <param name="amount">The amount to put to the itemSlot in the inventory.</param>
	public void PutItemInHandToItemSlot(GameObject itemSlot, int amount = -1)
	{
		Inventory inventory = WorldController.itemController.itemSlotInventories [itemSlot];

		if (itemSlot == null || inventory == null)
		{
			Debug.Log("Can't put item to null itemSlot or inventory.");
			return;
		}

		if(itemInHand == null)
		{
			Debug.Log("Can't put item to itemSlot if character isn't holding an item");
			return;
		}

		if(inventory.GetItemSlotID(itemSlot) < 0)
		{
			Debug.Log("The inventory doesn't contain the itemSlot");
			return;
		}

		Item itemToAdd = itemInHand.Copy();
		if (itemToAdd.StackSize > amount && amount > 0)
		{
			itemToAdd.Remove(itemToAdd.StackSize - amount);
		}

		int itemCountNotAdded = inventory.AddItem(itemToAdd, inventory.GetItemSlotID(itemSlot));

		if(itemToAdd.StackSize - itemCountNotAdded >= itemInHand.StackSize)
		{
			itemInHand = null;
			inventory.UpdateItemSlots ();
			UpdateItemInHand ();
		}
		else
		{
			itemInHand.Remove(itemToAdd.StackSize - itemCountNotAdded);
			inventory.UpdateItemSlots ();
			UpdateItemInHand ();
		}
	}

	/// <summary>
	/// Update itemInHand graphics
	/// </summary>
	public void UpdateItemInHand()
	{
		if (itemInHand != null && itemInHand.StackSize > 0)
		{
			if (itemInHandGameobject != null)
			{
				Image image = itemInHandGameobject.GetComponent<Image>();

				if (image == null)
				{
					Debug.Log("Why is character holding a non image gameObject named " + itemInHandGameobject.transform.name + "?");
				}
				else if(image.sprite == null)
				{
					Debug.Log("Why is character holding an image named " + image.transform.name + " without a sprite?");
				}
				else if (image.sprite.name != itemInHand.itemName)
				{
					image.sprite = itemInHand.itemSprite;
				}

				Text text = itemInHandGameobject.GetComponentInChildren<Text>();
				if(text == null)
				{
					Debug.Log("Why is there an item without itemCountNumber?");
					
					GameObject itemCountNum = GameObject.Instantiate(WorldController.itemController.itemCountNumber, Vector3.zero, Quaternion.identity);
					itemCountNum.GetComponent<Text>().text = itemInHand.StackSize.ToString();
					itemCountNum.GetComponent<RectTransform>().pivot = Vector2.zero;
					itemCountNum.transform.SetParent(itemInHandGameobject.transform, false);
				}
				else if(text.text != itemInHand.StackSize.ToString())
				{
					text.text = itemInHand.StackSize.ToString();
				}
			}
			else
			{
				WorldController.itemController.CreateItemGameObject (itemInHand, Vector3.zero, WorldController.itemController.inventory, inventory);
			}
		}
		else 
		{
			if (itemInHandGameobject != null)
			{
				GameObject.Destroy (itemInHandGameobject);
			}

			if(itemInHand != null)
			{
				itemInHand = null;
			}
		}
	}
}