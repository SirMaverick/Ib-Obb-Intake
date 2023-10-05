using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class BaseEnemyScript : MonoBehaviour {

	public WeakPoint weakPoint;
	public Enemy enemy;

	public virtual void Start() {
		weakPoint.SetEnemyScript(this);
		enemy.SetEnemyScript(this);
	}
	public abstract void EnemyHit(PlayerController player);
	public abstract void WeakPointHit(PlayerController player);
	public abstract void MoveWeakPoint();
	public abstract void MoveEnemy();
	public virtual void Move() {
		MoveWeakPoint();
		MoveEnemy();
	}
}

