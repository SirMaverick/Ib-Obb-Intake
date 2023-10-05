using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The script placed on the GameObject which has the model of the enemy.
/// This script contains the OnTriggerEnter that activates the EnemyHit function on the parent.
/// </summary>
public class Enemy : MonoBehaviour
{
    private BaseEnemyScript parent;

	public void SetEnemyScript(BaseEnemyScript enemyScript) {
		parent = enemyScript;
	}

	private void OnTriggerEnter(Collider other) {
		if(other.gameObject.layer == LayerMask.NameToLayer("Player")) {
			parent.EnemyHit(other.gameObject.GetComponent<PlayerController>());
		}
	}
}
