using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : MonoBehaviour
{
	Dictionary<string, Item> itemPrototypes;
	List<Item> itemsOnGround;
	Dictionary<GameObject, Item> itemObjectsOnGround;

	public Dictionary<string, Inventory> inventories;
	public Dictionary<GameObject, Inventory> itemSlotInventories;

	public GameObject itemCountNumber;
	public Transform inventory;


	void Awake()
	{
		CreateItemPrototypes();

		WorldController.itemController = this;
	}

	void Start ()
	{
		itemSlotInventories = new Dictionary<GameObject, Inventory> ();
		itemsOnGround = new List<Item>();
		itemObjectsOnGround = new Dictionary<GameObject, Item>();
		inventories = new Dictionary<string, Inventory>();

		if (itemPrototypes == null)
		{
			Debug.Log ("There are no item prototypes.");
		}
	}

	void Update ()
	{
		if (Input.GetMouseButtonDown (0) )
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (ray.origin, Vector2.zero);

			if (hit)
			{
				if (hit.transform.tag == "InventoryItem")
				{
					GameObject itemSlot = hit.transform.parent.gameObject;

					if (itemSlotInventories.ContainsKey (itemSlot) == false)
					{
						Debug.Log ("Why is there a GameObject with the tag InventoryItem which doesn't have an itemSlot as a parent?");
					}
					else if (WorldController.currentCharacter.itemInHand == null)
					{
						WorldController.currentCharacter.PickItemFromItemSlot (itemSlot);
						hit.transform.gameObject.GetComponent<BoxCollider2D> ().enabled = false;
					}
					else if (WorldController.currentCharacter.itemInHand.itemName == itemSlotInventories [itemSlot].GetItemNameFromItemslot (itemSlot))
					{
						WorldController.currentCharacter.PutItemInHandToItemSlot (itemSlot);
					}
				}
				else if (hit.transform.tag == "ItemSlot" && WorldController.currentCharacter.itemInHand != null)
				{
					WorldController.currentCharacter.PutItemInHandToItemSlot (hit.transform.gameObject);
				}
			}
		}
		else if(Input.GetMouseButtonDown (1))
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast (ray.origin, Vector2.zero);

			if (hit)
			{
				if (hit.transform.tag == "InventoryItem")
				{
					GameObject itemSlot = hit.transform.parent.gameObject;

					if (itemSlotInventories.ContainsKey (itemSlot) == false)
					{
						Debug.Log ("Why is there a GameObject with the tag InventoryItem which doesn't have an itemSlot as a parent?");
					}
					else if (WorldController.currentCharacter.itemInHand == null)
					{
						WorldController.currentCharacter.PickItemFromItemSlot (itemSlot, 1);
						hit.transform.gameObject.GetComponent<BoxCollider2D> ().enabled = false;
					}
					else if (WorldController.currentCharacter.itemInHand.itemName == itemSlotInventories [itemSlot].GetItemNameFromItemslot (itemSlot))
					{
						if (Input.GetButton ("Alternate Mode") == false)
						{
							if (WorldController.currentCharacter.itemInHand.StackSize < WorldController.currentCharacter.itemInHand.maxStackSize)
							{
								WorldController.currentCharacter.PickItemFromItemSlot (itemSlot, 1);
							}
						}
						else
						{
							WorldController.currentCharacter.PutItemInHandToItemSlot (itemSlot, 1);
						}
					}
				}
				else if (hit.transform.tag == "ItemSlot" && WorldController.currentCharacter.itemInHand != null)
				{
					WorldController.currentCharacter.PutItemInHandToItemSlot (hit.transform.gameObject, 1);
				}
			}
		}


		CharController currentCharacter = WorldController.currentCharacter;

		if(currentCharacter.selectedItem == null || (currentCharacter.characterSelectedItem == false && Vector2.Distance(currentCharacter.selectedItem.location, currentCharacter.transform.position) < currentCharacter.itemGrabDistance))
		{
			foreach(Item item in itemsOnGround)
			{
				if(Vector2.Distance(item.location, currentCharacter.transform.position) < currentCharacter.itemGrabDistance)
				{
					currentCharacter.selectedItem = item;
					currentCharacter.checkedGroundItemsThisFrame = true;
					currentCharacter.itemPickupText.SetActive(true);
				}
			}
		}

		if(currentCharacter.checkedGroundItemsThisFrame == false && currentCharacter.selectedItem != null &&currentCharacter.characterSelectedItem == false)
		{
			currentCharacter.selectedItem = null;
			currentCharacter.itemPickupText.SetActive(false);
		}

		if(Input.GetButtonDown("Interact") && currentCharacter.selectedItem != null && Vector2.Distance(currentCharacter.selectedItem.location, currentCharacter.transform.position) < currentCharacter.itemGrabDistance && currentCharacter.inventory.IsFull() == false)
		{
			int itemsNotAdded = currentCharacter.inventory.AddItem(currentCharacter.selectedItem);

			if(itemsNotAdded <= 0)
			{
				Destroy(currentCharacter.selectedItem.itemObject);
				itemsOnGround.Remove(currentCharacter.selectedItem);
				currentCharacter.selectedItem = null;
				currentCharacter.characterSelectedItem = false;
				currentCharacter.itemPickupText.SetActive(false);
			}
			else
			{
				currentCharacter.selectedItem.Remove(currentCharacter.selectedItem.StackSize - itemsNotAdded);
				UpdateItem(currentCharacter.selectedItem);
			}
		}


		currentCharacter.checkedGroundItemsThisFrame = false;
	}

	// TODO: This function should load itemPrototypes from xml
	void CreateItemPrototypes()
	{
		itemPrototypes = new Dictionary<string, Item>();
		
		itemPrototypes.Add("Dirt_floor", new Item("Dirt", WorldController.spriteManager.GetSprite("Dirt_floor"), 1, 30));
		itemPrototypes.Add("Grass_floor", new Item("Grass", WorldController.spriteManager.GetSprite("Grass_floor"), 1, 30));
		itemPrototypes.Add("Square", new Item("Square", WorldController.spriteManager.GetSprite("Square"), 1, 20));
	}
	
	/// <summary>
	/// Create an item on floor.
	/// </summary>
	/// <param name="itemName">The name of the item to create.</param>
	/// <param name="stackSize">The stackSize of the item to create,</param>
	/// <param name="location">The location of the item to create.</param>
	public void CreateItem(string itemName, int stackSize, Vector2 location)
	{
		if(itemPrototypes[itemName] == null)
		{
			Debug.Log("There isn't an itemPrototype named " + itemName);
			return;
		}

		Item itemPrototype = itemPrototypes[itemName];

		if(stackSize > itemPrototype.maxStackSize)
		{
			Debug.Log("You can't create an item with the stackSize of " + stackSize.ToString() + " when the maxStackSize is " + itemPrototype.maxStackSize.ToString() + ".");
			stackSize = itemPrototype.maxStackSize;
		}

		Item item = new Item(itemName, itemPrototype.itemSprite, stackSize, itemPrototype.maxStackSize);
		item.location = location;

		itemsOnGround.Add(item);
		CreateItemGameObject(item, location);
	}

	/// <summary>
	/// Create an item in the inventory.
	/// </summary>
	/// <param name="itemName">The name of the item to create.</param>
	/// <param name="stackSize">The stackSize of the item to create.</param>
	/// <param name="inventory">The inventory to create the item into</param>
	/// <param name="itemSlotID">The ID of the itemSlot to create the item into.</param>
	public void CreateItem(string itemName, int stackSize, Inventory inventory, int itemSlotID = -1)
	{
		if(itemPrototypes[itemName] == null)
		{
			Debug.Log("There isn't an itemPrototype named " + itemName);
			return;
		}

		Item itemPrototype = itemPrototypes[itemName];

		if(stackSize > itemPrototype.maxStackSize)
		{
			Debug.Log("You can't create an item with the stackSize of " + stackSize.ToString() + " when the maxStackSize is " + itemPrototype.maxStackSize.ToString() + ".");
			stackSize = itemPrototype.maxStackSize;
		}

		Item item = new Item(itemName, itemPrototype.itemSprite, stackSize, itemPrototype.maxStackSize);

		int itemsNotCreated = inventory.AddItem(item, itemSlotID);

		item.Remove(itemsNotCreated);

	}

	/// <summary>
	/// Creates a GameObject for the item.
	/// </summary>
	/// <param name="item">The item to create the GameObject for.</param>
	/// <param name="inventory">The inventory to create the gameobject to.</param>
	public void CreateItemGameObject(Item item, Vector3 location, Transform parent = null, Inventory inventory = null)
	{
		GameObject itemObject = new GameObject ("Item");
		item.itemObject = itemObject;

		BoxCollider2D boxCollider = itemObject.AddComponent<BoxCollider2D>();

		if (inventory != null)
		{
			// The GameObject has to be created to an inventory.
			RectTransform rt = itemObject.AddComponent<RectTransform>();
			rt.SetParent(parent, false);
			rt.position = new Vector3(location.x, location.y, location.z - 0.1f);
			rt.sizeDelta = new Vector2(80, 80);
			rt.pivot = new Vector2(0, 1);
			rt.tag = "InventoryItem";

			itemObject.AddComponent<CanvasRenderer> ();
			Image image = itemObject.AddComponent<Image>();
			image.sprite = item.itemSprite;

			boxCollider.size = rt.sizeDelta;
			boxCollider.offset = new Vector2(rt.sizeDelta.x / 2, -(rt.sizeDelta.y / 2));

			GameObject itemCountNum = GameObject.Instantiate (WorldController.itemController.itemCountNumber, Vector3.zero, Quaternion.identity);
			itemCountNum.GetComponent<Text> ().text = item.StackSize.ToString ();
			itemCountNum.GetComponent<RectTransform> ().pivot = Vector2.zero;
			itemCountNum.transform.SetParent (itemObject.transform, false);

		}
		else
		{
			// The GameObject has to be created on the ground.
			if (parent != null)
			{
				itemObject.transform.SetParent(parent, false);
			}
			else
			{
				itemObject.transform.SetParent(transform, false);
			}

			itemObject.transform.tag = "GroundItem";
			itemObject.transform.position = new Vector3(location.x, location.y, location.z - 0.1f);

			SpriteRenderer sr = itemObject.AddComponent<SpriteRenderer>();
			sr.sprite = item.itemSprite;
			sr.sortingLayerName = "Items";

			GameObject canvasObjact = new GameObject ("ItemCount");
			canvasObjact.transform.SetParent (itemObject.transform);
			Canvas canvas = canvasObjact.AddComponent<Canvas> ();
			canvas.renderMode = RenderMode.WorldSpace;
			canvas.sortingLayerName = "Items";
			canvas.sortingOrder = 1;
			canvasObjact.GetComponent<RectTransform> ().sizeDelta = Vector2.one;

			GameObject itemCountNum = GameObject.Instantiate (WorldController.itemController.itemCountNumber, Vector3.zero, Quaternion.identity);
			itemCountNum.GetComponent<Text> ().text = item.StackSize.ToString ();
			itemCountNum.GetComponent<RectTransform> ().pivot = Vector2.zero;
			itemCountNum.transform.SetParent (canvasObjact.transform, false);
			itemCountNum.transform.localScale = new Vector3 (0.0125f, 0.0125f);


			itemObjectsOnGround.Add(itemObject, item);
		}
	}

	/// <summary>
	/// Update the items itemObject.
	/// </summary>
	public void UpdateItem (Item item)
	{
		if (item != null && item.StackSize > 0)
		{
			GameObject itemObject = item.itemObject;
			if (itemObject != null)
			{
				// The item has an itemObject so update it.
				SpriteRenderer sr = itemObject.GetComponent<SpriteRenderer>();

				if (sr == null)
				{
					Debug.Log("Why isn't there an SpriteRenderer caoponent on the itemObject named " + itemObject.transform.name + "?");
				}
				else if(sr.sprite == null)
				{
					Debug.Log("Why isn't there a sprite on a SpriteRenderer named " + sr.transform.name + "?");
				}
				else if (sr.sprite.name != item.itemName)
				{
					sr.sprite = item.itemSprite;
				}

				Text text = itemObject.GetComponentInChildren<Text>();
				if(text == null)
				{
					Debug.Log("Why is there an item without itemCountNumber?");
					
					GameObject itemCountNum = GameObject.Instantiate(WorldController.itemController.itemCountNumber, Vector3.zero, Quaternion.identity);
					itemCountNum.GetComponent<Text>().text = item.StackSize.ToString();
					itemCountNum.GetComponent<RectTransform>().pivot = Vector2.zero;
					itemCountNum.transform.SetParent(itemObject.transform, false);
				}
				else if(text.text != item.StackSize.ToString())
				{
					text.text = item.StackSize.ToString();
				}
			}
			else
			{
				// The item doesn't have an itemObject so if possible create it.
				if (item.itemSlot != null && item.inventory != null)
				{
					// Create in an inventory.
					CreateItemGameObject (item, item.itemSlot.transform.position, item.itemSlot.transform, item.inventory);
				}
				else if (item.location != Vector2.zero)
				{
					// Create on ground
					CreateItemGameObject (item, item.location, null, null);
				}
				else
				{
					Debug.Log ("The item doesn't have an itemObject and it can't be created.");
				}
			}
		}
	}
}
