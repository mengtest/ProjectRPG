using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
	public static SpriteManager spriteManager;
	public static WorldController worldController;
	public static ItemController itemController;

	public static CharController currentCharacter;

	void Awake ()
	{
		worldController = this;
	}
}
