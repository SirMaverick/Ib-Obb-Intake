using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The script placed on the GameObject which contains the weakpoint of the enemy.
/// This script contains the OnTriggerEnter that activates the WeakPointHit function on the parent.
/// </summary>
public class WeakPoint : MonoBehaviour
{
	private BaseEnemyScript parent;

	public void SetEnemyScript(BaseEnemyScript enemyScript) {
		parent = enemyScript;
	}

	private void OnTriggerEnter(Collider other) {
		if(other.gameObject.layer == LayerMask.NameToLayer("Player")) {
			parent.WeakPointHit(other.gameObject.GetComponent<PlayerController>());
		}
	}
}
