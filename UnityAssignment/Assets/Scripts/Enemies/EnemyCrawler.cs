using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An enemy that crawls over the floor within boundaries.
/// The script derives from the base EnemyScript and overrides certain functions.
/// This script moves the GameObjects that contain the model and the 'weakpoint' of the enemy
/// </summary>
public class EnemyCrawler : BaseEnemyScript
{
    [SerializeField] private Vector3 startingPoint;
    [SerializeField] private float leftMovementBoundary, rightMovementBoundary;
    [SerializeField] private bool goLeft, goRight;
    [SerializeField] private float movementSpeed = 1.0f;
    [SerializeField] private float direction = 1;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        startingPoint = transform.position;
        goRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public override void EnemyHit(PlayerController player) {
        player.GetHit();
	}

    public override void WeakPointHit(PlayerController player) {
        Destroy(gameObject);
	}

	public override void Move() {
		base.Move();
	}

	public override void MoveWeakPoint() {
        Vector3 _currentPosition = weakPoint.transform.position;
        if(_currentPosition.x + (movementSpeed * direction * Time.deltaTime) > startingPoint.x + rightMovementBoundary && goRight) {
            goRight = false;
            goLeft = true;
            direction = -1;
        } else if(_currentPosition.x - (movementSpeed * direction * Time.deltaTime) < startingPoint.x - leftMovementBoundary && goLeft) {
            goRight = true;
            goLeft = false;
            direction = 1;
        }
        Vector3 _newPosition = weakPoint.transform.position;
        _newPosition.x += movementSpeed * direction * Time.deltaTime;
        weakPoint.transform.position = _newPosition;
    }

	public override void MoveEnemy() {
        Vector3 _currentPosition = enemy.transform.position;
        if(_currentPosition.x + (movementSpeed * direction * Time.deltaTime) > startingPoint.x + rightMovementBoundary && goRight) {
            goRight = false;
            goLeft = true;
            direction = -1;
        } else if(_currentPosition.x - (movementSpeed * direction * Time.deltaTime) < startingPoint.x - leftMovementBoundary && goLeft) {
            goRight = true;
            goLeft = false;
            direction = 1;
        }
        Vector3 _newPosition = enemy.transform.position;
        _newPosition.x += movementSpeed * direction * Time.deltaTime;
        enemy.transform.position = _newPosition;
    }
}
