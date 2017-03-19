using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class SpriteManager : MonoBehaviour
{
	// The dictionary where all sprites are held.
	Dictionary<string, Sprite> sprites;

	void Awake()
	{
		WorldController.spriteManager = this;
		LoadSprites();
	}

	// Loads all the sprites from the Textures folder in StreamingAssets.
	void LoadSprites ()
	{
		sprites = new Dictionary<string, Sprite> ();

		string filePath = Path.Combine (Application.streamingAssetsPath, "Textures");
		LoadSpritesFromDirectory (filePath);
	}

	// Loads the sprites from a directory and its subfolders.
	void LoadSpritesFromDirectory (string filePath)
	{
		// Are there any subdirectories? If there are load sprites from there too.
		string [] subDirs = Directory.GetDirectories (filePath);

		foreach (string sd in subDirs)
		{
			LoadSpritesFromDirectory (sd);
		}

		string [] filesInDir = Directory.GetFiles (filePath);
		foreach (string fn in filesInDir)
		{
			LoadImage (fn);
		}
	}

	// Load an image file from a filePath.
	void LoadImage (string filePath)
	{
		// Check if it is a png. If not it is not a supported texture file and return.
		if (filePath.EndsWith (".png") == false)
		{
			return;
		}

		// Load the file into a texture
		byte [] imageBytes = File.ReadAllBytes (filePath);
		Texture2D imageTexture = new Texture2D (2, 2);

		if (imageTexture.LoadImage (imageBytes))
		{
			// Image was successfully loaded.
			string baseSpriteName = Path.GetFileNameWithoutExtension (filePath);
			
			LoadSprite (baseSpriteName, imageTexture, new Rect (0, 0, imageTexture.width, imageTexture.height), imageTexture.width);
		}
	}

	// Creates a sprite from a texture2D and given properties.
	void LoadSprite (string spriteName, Texture2D imageTexture, Rect spriteCoordinates, int pixelsPerUnit)
	{
		Vector2 pivotPoint = new Vector2 (0.5f, 0.5f);
		Sprite s = Sprite.Create (imageTexture, spriteCoordinates, pivotPoint, pixelsPerUnit);

		s.texture.filterMode = FilterMode.Point;

		sprites.Add(spriteName, s);
	}

	/// <summary>
	/// It finds the sprite with spriteName and returns it.
	/// </summary>
	/// <param name="spriteName">The name of the sprite to look for.</param>
	/// <returns>The sprite with the spriteName.</returns>
	public Sprite GetSprite (string spriteName)
	{
		if (sprites.ContainsKey (spriteName) == false)
		{
			Debug.LogError ("No sprite with name: " + spriteName + ". Returning the Default sprite.");
			return sprites ["Default"];
		}

		return sprites [spriteName];
	}
}
