using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		InitPosition ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	// Sets the Player position to a free tile around the center of the map.
	private void InitPosition ()
	{
		int gameRows = GameManager.instance.boardManager.rows;
		int gameColumns = GameManager.instance.boardManager.columns;
		Vector3 position;
		int tilesFromCenter = 0;
		do {
			int rowMin = gameRows / 2 - tilesFromCenter;
			int rowMax = gameRows / 2 + tilesFromCenter;
			int columnMin = gameColumns / 2 - tilesFromCenter;
			int columnMax = gameColumns / 2 + tilesFromCenter;
			int tries = 0;
			do {
				int row = Random.Range (rowMin - 1, rowMax);
				int column = Random.Range (columnMin - 1, columnMax);
				position = new Vector3 (column, row);
				tries++;
			} while (!GameManager.instance.boardManager.isFreePosition (position) && tries < 10);
			tilesFromCenter++;
		} while(!GameManager.instance.boardManager.isFreePosition (position));
		transform.position = position;
	}
}
