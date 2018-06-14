using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollideIncreaseDrag : MonoBehaviour {
    public float factor;

    private void OnCollisionEnter(Collision collision) {    
        var disk = collision.gameObject.GetComponent<Disk>();
        var powerUp = collision.gameObject.GetComponent<PowerUp>();
        if (!powerUp) {
            Debug.Log("Didnt find powerUp");
        }

        
        if (!Board.Instance.isYourTurn || !disk) {
            return;
        }

        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.drag += factor;
    }
}
