using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
	public float defaultSpeed = 1f;
	public float rotateSpeed = 20f;
	public float defaultRotation = -90;
	public LayerMask blockingLayer;
	// Move directions, from -1 to 1
	private int xDir;
	private int yDir;
	private Vector3 targetPosition;
	private Animator animator;

	// Use this for initialization
	void Start ()
	{
		animator = GetComponent <Animator> ();
		InitPosition ();
		targetPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
		xDir = 0;
		yDir = 0;
		if (Input.GetKey (KeyCode.DownArrow)) {
			yDir = -1;
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			yDir = 1;
		}
		if (Input.GetKey (KeyCode.LeftArrow)) {
			xDir = -1;
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			xDir = 1;
		}
		// Keep moving to the target position before changing direction or stopping.
		if (AllowDirectionChange ()) {
			if (xDir == 0 && yDir == 0) {
				SetMoving (false);
			} else {
				// Calculate the new target position in the input direction.
				// If the Player can move to this direction, update the target position.
				// Otherwise, switch to the idle state.
				Vector3 newTargetPosition = GetTargetPosition ();
				if (CanMoveTo (newTargetPosition)) {
					targetPosition = newTargetPosition;
					// Move towards the target position.
					Move ();
				} else {
					SetMoving (false);
				}
			}
		} else {
			Move ();
		}
	}

	// Allow direction change if it's in the opposite of the current direction.
	private bool AllowDirectionChange ()
	{
		bool allowDirChange;
		if (transform.position == targetPosition) {
			allowDirChange = true;
		} else {
			Vector3 dirToTarget = targetPosition - transform.position;
			Vector3 currentDir = new Vector3 (xDir, yDir);
			allowDirChange = Vector3.Dot (dirToTarget, currentDir) < 0;
		}
		return allowDirChange;
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

	// Calculates the target position, from the current position and the x and y directions.
	private Vector3 GetTargetPosition ()
	{
		// Snap to grid
		Vector3 currentPosition = new Vector3 (Mathf.Round (transform.position.x),
			                          Mathf.Round (transform.position.y),
			                          Mathf.Round (transform.position.z));
		return currentPosition + new Vector3 (xDir, yDir);
	}

	// Tests whether the Player can move toward its target position.
	// It can not move to an occupied position.
	// It can not move to a position blocked by the blocking layer.
	private bool CanMoveTo (Vector3 position)
	{
		bool canMove = GameManager.instance.boardManager.isFreePosition (position);
		if (canMove) {
			RaycastHit2D hit = Physics2D.Linecast (transform.position, position, blockingLayer);
			canMove = hit.collider == null;
		}
		return canMove;
	}

	// Moves the Enemy toward its target.
	private void Move ()
	{
		SetMoving (true);
		UpdateRotation ();
		transform.position = Vector3.MoveTowards (transform.position, targetPosition, defaultSpeed * Time.deltaTime);
	}

	// Rotates the sprite according to its current and target positions.
	private void UpdateRotation ()
	{
		Vector3 dir = targetPosition - transform.position;
		float angle = Mathf.Atan2 (dir.y, dir.x) * Mathf.Rad2Deg + defaultRotation;
		// Get a 0-360 angle
		if (angle < 0) {
			angle += 360;
		} else if (angle > 360) {
			angle -= 360;
		}
		float relativeAngle = angle - transform.rotation.eulerAngles.z;
		// Don't turn more than a U-turn
		if (relativeAngle > 180) {
			relativeAngle -= 360;
		}
		// 1 degree is ok
		if (Mathf.Abs (relativeAngle) > 1) {
			transform.Rotate (Vector3.forward, relativeAngle * Time.deltaTime * rotateSpeed);
		}
	}

	// Updates the moving state and animation of the Enemy.
	private void SetMoving (bool moving)
	{
		animator.SetBool ("moving", moving);
	}
}
