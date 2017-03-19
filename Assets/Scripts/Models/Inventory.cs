using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory
{
	List<GameObject> itemSlots;
	Dictionary<GameObject, Item> items;

	// This is null if the inventory doesn't belong to a character.
	public CharController character;

	public bool isOpened = false;
	public Action<Inventory> onOpened;
	public Action<Inventory> onClosed;

	ItemController itemController;


	public Inventory (List<GameObject> itemSlots, Action<Inventory> onOpened, Action<Inventory> onClosed, CharController character = null)
	{
		itemController = WorldController.itemController;

		this.itemSlots = itemSlots;
		this.character = character;

		this.onOpened += onOpened;
		this.onClosed += onClosed;

		items = new Dictionary<GameObject, Item> ();
		foreach (GameObject g in itemSlots)
		{
			items.Add (g, null);
			itemController.itemSlotInventories.Add (g, this);
		}

		itemController.inventories.Add ("Inventory" + (itemController.inventories.Count + 1).ToString (), this);
	}

	/// <summary>
	/// Adds the item to the itemSlot with itemSlotID. Use itemController.CreateItem instead.
	/// </summary>
	public int AddItem (Item item, int itemSlotID = -1)
	{
		if (itemSlotID < 0)
		{
			// The item can be put into any itemSlot.

			// The number of items to add.
			int itemCountToAdd = item.StackSize;

			// First try to add to an existing item
			foreach (GameObject g in itemSlots)
			{
				if (items [g] != null && items [g].itemName == item.itemName && itemCountToAdd > 0)
				{
					itemCountToAdd = items [g].Add (itemCountToAdd);

					if (itemCountToAdd <= 0)
					{
						if (character != null || isOpened)
						{
							UpdateItemSlots ();
						}
						return 0;
					}
				}
			}

			// Try to add to an empty itemSlot
			foreach (GameObject g in itemSlots)
			{
				if (items [g] == null && itemCountToAdd > 0)
				{
					items [g] = item.Copy ();
					items [g].itemSlot = g;
					items [g].inventory = this;

					if (character != null || isOpened)
					{
						UpdateItemSlots ();
					}
					return 0;
				}
			}

			if (character != null || isOpened)
			{
				UpdateItemSlots ();
			}
			return itemCountToAdd;
		}

		// The item has to be put into a spesific itemSlot.
		GameObject itemSlot = itemSlots [itemSlotID];
		if (itemSlot == null)
		{
			Debug.Log ("There isn't an itemSlot with the id " + itemSlotID.ToString ());
			return (item.StackSize);
		}

		if (items [itemSlot] == null)
		{
			// The itemSlot is empty.
			items [itemSlot] = item.Copy ();
			items [itemSlot].itemSlot = itemSlot;
			items [itemSlot].inventory = this;

			if (character != null || isOpened)
			{
				UpdateItemSlots ();
			}
			return 0;
		}
		else if (items [itemSlot].itemName != item.itemName)
		{
			Debug.Log ("The itemSlot can't take the item called " + item.itemName + ".");
			return item.StackSize;
		}
		else
		{
			return items [itemSlot].Add (item.StackSize);
		}
	}

	/// <summary>
	/// Updates the graphics of all items in the itemslots in this inventory.
	/// </summary>
	public void UpdateItemSlots ()
	{
		foreach (GameObject g in itemSlots)
		{
			if (items [g] != null && items [g].StackSize > 0)
			{
				// There is an item in the itemSlot so it needs to be updated.

				GameObject itemObject = null;

				// Check if the itemSlot has the items[g].itemObject as a child.
				foreach (Transform obj in g.transform.GetComponentsInChildren<Transform> ())
				{
					if (obj == items [g].itemObject)
					{
						itemObject = obj.gameObject;
						break;
					}
				}

				if (itemObject != null)
				{
					// The itemSlot has the items[g].itemObject as a child.
					Image image = itemObject.GetComponent<Image> ();

					// Check the image.
					if (image == null)
					{
						Debug.Log ("Why doesn't the itemObject have an image?");
						itemObject.AddComponent<Image> ().sprite = items [g].itemSprite;
					}
					else if (image.sprite == null || image.sprite.name != items [g].itemName)
					{
						image.sprite = items [g].itemSprite;
					}

					Text text = itemObject.GetComponentInChildren<Text> ();
					// Update the itemCountNumber.
					if (text == null)
					{
						Debug.Log ("Why is there an item without itemCountNumber?");

						GameObject itemCountNum = GameObject.Instantiate (itemController.itemCountNumber, Vector3.zero, Quaternion.identity);
						itemCountNum.GetComponent<Text> ().text = items [g].StackSize.ToString ();
						itemCountNum.GetComponent<RectTransform> ().pivot = Vector2.zero;
						itemCountNum.transform.SetParent (itemObject.transform, false);
					}
					else if (text.text != items [g].StackSize.ToString ())
					{
						text.text = items [g].StackSize.ToString ();
					}
				}
				else
				{
					// The itemSlot doesn't have the items[g].itemObject as a child so create it.
					itemController.CreateItemGameObject (items [g], g.transform.position, g.transform, this);
				}
			}
			else
			{
				// The itemSlot doesn't have an item or its stackSize is less than 1.
				if (items [g] != null)
				{
					if (items [g].itemObject != null)
					{
						GameObject.Destroy (items [g].itemObject);
					}
					items [g] = null;
				}

				if (g.transform.childCount > 0)
				{
					for (int i = g.transform.childCount - 1; i >= 0; i--)
					{
						GameObject.Destroy (g.transform.GetChild (i).gameObject);
					}
				}
			}
		}
	}

	/// <summary>
	/// Gets the item from itemSlot and if it needs the whole item it, then removes it from the inventory.
	/// </summary>
	/// <param name="itemSlot">The itemSlot to get the item from.</param>
	/// <param name="amount">The amount of items to get from the itemSlot.</param>
	/// <returns>The item from the itemSlot with the amount.</returns>
	public Item GetItemFromItemSlot (GameObject itemSlot, int amount = -1)
	{
		if (itemSlot == null)
		{
			Debug.Log ("Can't pick item from null itemSlot.");
			return null;
		}

		if (items [itemSlot] == null || items [itemSlot].StackSize <= 0)
		{
			Debug.Log ("The itemSlot is empty or it isn't an itemSlot.");
			return null;
		}

		Item item;

		if (items [itemSlot].StackSize > amount && amount > 0)
		{
			// Don't need to get the whole item.
			item = items [itemSlot].Copy ();
			item.itemSlot = null;
			item.inventory = null;
			item.Remove (item.StackSize - amount);
			items [itemSlot].Remove (item.StackSize);
		}
		else
		{
			// Get the whole item.
			item = items [itemSlot];
			items[itemSlot] = null;
			item.itemSlot = null;
			item.inventory = null;
		}
		return item;
	}

	/// <summary>
	/// Gets the ID of the itemSlot.
	/// </summary>
	/// <param name="itemSlot">The itemSlot to get the ID for.</param>
	/// <returns>The ID of the itemSlot.</returns>
	public int GetItemSlotID (GameObject itemSlot)
	{
		if (itemSlot == null || itemSlots.Contains (itemSlot) == false)
		{
			Debug.Log ("Can't get an itemSlotID if the itemSlot doesn't exist.");
			return -1;
		}

		return itemSlots.IndexOf (itemSlot);
	}

	/// <summary>
	/// Gets the name of the item in the itemSlot.
	/// </summary>
	/// <param name="itemSlot">The itemSlot to get the name of the item from.</param>
	/// <returns>The name of the item from the itemSlot.</returns>
	public string GetItemNameFromItemslot (GameObject itemSlot)
	{
		if (itemSlot == null || itemSlots.Contains (itemSlot) == false)
		{
			Debug.Log ("This isn't an itemSlot.");
			return null;
		}

		if (items [itemSlot] == null)
		{
			return null;
		}

		return items [itemSlot].itemName;
	}

	/// <summary>
	/// Checks if there are any empty itemSlots.
	/// </summary>
	public bool IsFull ()
	{
		if (items.ContainsValue (null))
		{
			return false;
		}
		else
		{
			return true;
		}
	}
}
