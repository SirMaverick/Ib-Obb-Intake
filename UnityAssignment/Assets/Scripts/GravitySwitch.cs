using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The gravity switch that only interacts with players.
/// This script only sends a message to the player that they entered a switch.
/// Everything else is handled by the player.
/// </summary>
public class GravitySwitch : MonoBehaviour
{
    void OnTriggerEnter(Collider c) {
        //Only works on the BoxCollider of the character, not the characterController his collider.
        if(c is BoxCollider) {
            if(c.transform.GetComponent<PlayerController>() != null) {
                PlayerController player = c.transform.GetComponent<PlayerController>();
                {
                    player.Switch();
                }
            }
		}
    }
}
