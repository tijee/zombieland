using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;

	public BoardManager boardScript;

	private List<Enemy> enemies;

	// Call this to add the passed in Enemy to the List of Enemy objects.
	public void AddEnemy (Enemy script)
	{
		enemies.Add (script);
	}

	void Awake ()
	{
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		DontDestroyOnLoad (gameObject);

		enemies = new List<Enemy> ();
		boardScript = GetComponent<BoardManager> ();
		InitGame ();
	}

	void InitGame ()
	{
		boardScript.SetupScene ();
		boardScript.SetupCamera ();
	}

	// Update is called every frame.
	void Update ()
	{
		
	}
}
