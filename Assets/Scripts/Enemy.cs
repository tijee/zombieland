using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
	// Speed in unit / second
	public float defaultSpeed = 0.5f;
	public float attackSpeed = 1f;
	public float rotateSpeed = 20f;
	public float defaultRotation = 90;
	// Time in idle state in seconds
	public float idleTime = 2f;

	public LayerMask blockingLayer;

	// Move directions, from -1 to 1
	private int xDir;
	private int yDir;
	private bool idle = false;
	private Vector3 targetPosition;

	private Animator animator;

	void Start ()
	{
		animator = GetComponent <Animator> ();
		GameManager.instance.AddEnemy (this);
		targetPosition = transform.position;
	}

	void Update ()
	{
		if (!idle) {
			if (transform.position == targetPosition) {
				if (xDir == 0 && yDir == 0) {
					// The Enemy is not moving, choose a new direction.
					ChangeDirection ();
				} else {
					// Calculate the new target position in the same direction.
					// If the Enemy can move to the same direction, update the target position.
					// Otherwise, switch to the idle state.
					Vector3 newTargetPosition = GetTargetPosition ();
					if (CanMoveTo (newTargetPosition)) {
						NewTargetPosition (newTargetPosition);
					} else {
						StartCoroutine (SetIdle ());
					}
				}
			} else {
				// Move towards the target position.
				Move ();
			}
		}
	}

	// Tests whether the Enemy can move toward its target position.
	// It can not move to an occupied position.
	// It can not move to a position block by the blocking layer.
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

	// Changes the direction of the Enemy.
	private void ChangeDirection ()
	{
		Vector3 newTargetPosition;
		int i = 0;
		bool newDirectionFound = false;
		do {
			RandomDirection ();
			newTargetPosition = GetTargetPosition ();
			newDirectionFound = CanMoveTo (newTargetPosition);
			i++;
		} while (!newDirectionFound && i < 35);	// With 35 tries we have 99% chances to find an exit
		if (newDirectionFound) {
			NewTargetPosition (newTargetPosition);
		}
	}

	// Updates x and y directions randomly.
	private void RandomDirection ()
	{
		do {
			xDir = Random.Range (-1, 2);
			yDir = Random.Range (-1, 2);
		} while (xDir == 0 && yDir == 0);
	}

	// Clears the x and y directions.
	private void ClearDirection ()
	{
		xDir = 0;
		yDir = 0;
	}

	// Sets the Enemy to idle state.
	private IEnumerator SetIdle ()
	{
		idle = true;
		SetMoving (false);
		ClearDirection ();
		yield return new WaitForSeconds (idleTime);
		idle = false;
	}

	// Calculates the target position, from the current position and the x and y directions.
	private Vector3 GetTargetPosition ()
	{
		return transform.position + new Vector3 (xDir, yDir);
	}

	// Updates the target position.
	private void NewTargetPosition (Vector3 position)
	{
		GameManager.instance.boardManager.RemoveEnemyPosition (targetPosition);
		targetPosition = position;
		GameManager.instance.boardManager.AddEnemyPosition (targetPosition);
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
