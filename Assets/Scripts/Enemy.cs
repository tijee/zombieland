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

	[HideInInspector] public Vector3 targetPosition;

	// Move directions, from -1 to 1
	private int xDir;
	private int yDir;
	private bool idle = false;
	private bool moving = true;

	private Animator animator;
	private BoxCollider2D boxCollider;

	void Start ()
	{
		animator = GetComponent <Animator> ();
		boxCollider = GetComponent <BoxCollider2D> ();
		ChangeDirection ();
		GameManager.instance.AddEnemy (this);
	}

	void Update ()
	{
		if (!idle) {
			if (transform.position == targetPosition) {
				// Calculate the new target position.
				UpdateTargetPosition ();
			}
			// Move towards the target position. If it is not possible, change direction for the next update.
			if (CanMove ()) {
				UpdateRotation ();
				Move ();
			} else {
				ChangeDirection ();
				if (moving) {
					StartCoroutine (SetIdle ());
				}
			}
		}
	}

	// Tests whether the Enemy can move toward its current target.
	private bool CanMove ()
	{
		boxCollider.enabled = false;
		RaycastHit2D hit = Physics2D.Linecast (transform.position, targetPosition, blockingLayer);
		boxCollider.enabled = true;
		return hit.collider == null;
	}

	// Moves the Enemy toward its target.
	// Returns true if the movement was successful, false if it was impossible due to a blocking item.
	private void Move ()
	{
		SetMoving (true);
		transform.position = Vector3.MoveTowards (transform.position, targetPosition, defaultSpeed * Time.deltaTime);
	}

	// Changes the direction of the Enemy.
	private void ChangeDirection ()
	{
		do {
			xDir = Random.Range (-1, 2);
			yDir = Random.Range (-1, 2);
		} while (xDir == 0 && yDir == 0);
		UpdateTargetPosition ();
	}

	private IEnumerator SetIdle ()
	{
		idle = true;
		SetMoving (false);
		yield return new WaitForSeconds (idleTime);
		idle = false;
	}

	// Calculates a new target position from the current position and the move direction.
	private void UpdateTargetPosition ()
	{
		// Snap to grid
		Vector3 currentPosition = new Vector3 (Mathf.Round (transform.position.x),
			                          Mathf.Round (transform.position.y),
			                          Mathf.Round (transform.position.z));
		targetPosition = currentPosition + new Vector3 (xDir, yDir);
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
		this.moving = moving;
		animator.SetBool ("moving", moving);
	}
}
