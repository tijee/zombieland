using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
	public int columns = 15;
	public int rows = 15;
	public int enemyCount = 5;

	public GameObject[] floorTiles;
	public GameObject[] wallTiles;
	public GameObject enemyTile;

	private List<GameObject> outerWalls = new List<GameObject> ();
	private List <Vector3> randomGridPositions = new List <Vector3> ();
	private List <Vector3> occupiedPositions = new List <Vector3> ();

	private Transform boardHolder;

	// Sets up the outer walls and floor (background) of the game board.
	void BoardSetup ()
	{
		boardHolder = new GameObject ("Board").transform;
		for (int x = -1; x <= columns; x++) {
			for (int y = -1; y <= rows; y++) {
				GameObject tileChoice = null;
				bool outerWall = false;
				if (x < 0 || x == columns || y < 0 || y == rows) {
					// Outer wall
					tileChoice = wallTiles [Random.Range (0, wallTiles.Length)];
					outerWall = true;
				} else {
					// Floor
					tileChoice = floorTiles [Random.Range (0, floorTiles.Length)];
				}
				Vector3 position = new Vector3 (x, y, 0f);
				GameObject tile = Instantiate (tileChoice, position, Quaternion.identity) as GameObject;
				tile.transform.SetParent (boardHolder);
				if (outerWall) {
					outerWalls.Add (tile);
					occupiedPositions.Add (position);
				}
			}
		}
	}

	// Clears our list gridPositions and prepares it to generate a new board.
	void InitGridPositions ()
	{
		randomGridPositions.Clear ();
		// Ignore first and last columns / rows (outer wall)
		for (int x = 1; x < columns - 1; x++) {
			for (int y = 1; y < rows - 1; y++) {
				randomGridPositions.Add (new Vector3 (x, y, 0f));
			}
		}
	}

	// Returns a random position from our list randomGridPositions.
	Vector3 RandomPosition ()
	{
		int randomIndex = Random.Range (0, randomGridPositions.Count);
		Vector3 randomPosition = randomGridPositions [randomIndex];
		randomGridPositions.RemoveAt (randomIndex);
		return randomPosition;
	}

	// Randomly creates the given number of objects on the grid.
	void LayoutObjectAtRandom (GameObject[] tiles, int count)
	{
		for (int i = 0; i < count; i++) {
			Vector3 randomPosition = RandomPosition ();
			GameObject tileChoice = tiles [Random.Range (0, tiles.Length)];
			GameObject tile = Instantiate (tileChoice, randomPosition, Quaternion.identity) as GameObject;
			tile.transform.SetParent (boardHolder);
			occupiedPositions.Add (randomPosition);
		}
	}

	// Convenient method to pass only one tile.
	void LayoutObjectAtRandom (GameObject tile, int count)
	{
		LayoutObjectAtRandom (new GameObject[]{ tile }, count);
	}

	// Randomly replaces an outer wall with a ground tile to do the exit.
	void LayoutExit ()
	{
		int index;
		Vector3[] corners = {
			new Vector3 (-1, -1),
			new Vector3 (-1, rows),
			new Vector3 (rows, columns),
			new Vector3 (columns, -1)
		};
		Vector3 exitPosition;
		do {
			index = Random.Range (0, outerWalls.Count);
			GameObject exitOuterWall = outerWalls [index];
			exitPosition = exitOuterWall.transform.position;
		} while (corners.Contains (exitPosition));
		Destroy (outerWalls [index]);
		GameObject exitFloor = floorTiles [Random.Range (0, floorTiles.Length)];
		GameObject exitTile = Instantiate (exitFloor, exitPosition, Quaternion.identity) as GameObject;
		exitTile.transform.SetParent (boardHolder);
	}

	// Initializes our level and calls the previous functions to lay out the game board.
	public void SetupScene ()
	{
		BoardSetup ();
		InitGridPositions ();
		int wallTilesCount = columns * rows / 4;
		LayoutObjectAtRandom (wallTiles, wallTilesCount);
		LayoutObjectAtRandom (enemyTile, enemyCount);
		LayoutExit ();
	}

	// Sets the camera viewport to see the whole level.
	public void SetupCamera ()
	{
		int margin = 2;
		Camera.main.transform.position = new Vector3 (columns / 2, rows / 2, -10);
		Camera.main.orthographicSize = Mathf.Max (columns, rows) / 2 + margin;
	}

	// Adds the given position to the list of enemy positions.
	public void AddEnemyPosition (Vector3 position)
	{
		lock (occupiedPositions) {
			if (!occupiedPositions.Contains (position)) {
				occupiedPositions.Add (position);
			}
		}
	}

	// Removes the given position from the list of enemy positions.
	public void RemoveEnemyPosition (Vector3 position)
	{
		lock (occupiedPositions) {
			occupiedPositions.Remove (position);
		}
	}

	// Returns true if the given position is occupied by a wall or an enemy, false otherwise.
	public bool isFreePosition (Vector3 position)
	{
		lock (occupiedPositions) {
			return !occupiedPositions.Contains (position);
		}
	}
}
