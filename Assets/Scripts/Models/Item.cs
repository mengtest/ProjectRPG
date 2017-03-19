using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
	public string itemName;
	public Sprite itemSprite;
	private int stackSize;
	public int maxStackSize;

	public GameObject itemSlot;
	public Vector2 location;
	public Inventory inventory;
	public GameObject itemObject;

	public int StackSize
	{
		get
		{
			return stackSize;
		}

		protected set
		{
			stackSize = value;
		}
	}

	public Item (string itemName, Sprite itemSprite, int stackSize = 1, int maxStackSize = 30)
	{
		this.itemName = itemName;
		this.stackSize = stackSize;
		this.maxStackSize = maxStackSize;

		if(itemSprite != null)
		{
			this.itemSprite = itemSprite;
		}
		else
		{
			itemSprite = WorldController.spriteManager.GetSprite("Default");
		}
	}

	/// <summary>
	/// Copies current item.
	/// </summary>
	/// <returns>the item copied.</returns>
	public Item Copy()
	{
		Item item = new Item (itemName, itemSprite, stackSize, maxStackSize);
		item.itemSlot = itemSlot;
		item.location = location;
		item.inventory = inventory;
		item.itemObject = itemObject;
		return item;
	}

	/// <summary>
	/// Adds the amount to the item. 
	/// </summary>
	/// <param name="amount">The amount to add to the item.</param>
	/// <returns>The amount not added.</returns>
	public int Add (int amount)
	{
		if (stackSize >= maxStackSize)
			return amount;

		if (maxStackSize - stackSize < amount)
		{
			int returnAmount = amount - (maxStackSize - stackSize);

			stackSize = maxStackSize;			
			return returnAmount;
		}


		stackSize += amount;
		return 0;
	}

	/// <summary>
	/// Removes the amount from the item.
	/// </summary>
	/// <param name="amount">The amount to remove from the item.</param>
	/// <returns>The amount not removed.</returns>
	public int Remove (int amount)
	{
		if(stackSize - amount < 0)
		{
			stackSize = 0;
			return amount - stackSize;
		}

		stackSize -= amount;
		return 0;
	}
}
