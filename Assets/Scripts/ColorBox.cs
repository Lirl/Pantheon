using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBox : MonoBehaviour {

    public Material[] players;
    private int alliance = 0;
        
    void OnTriggerEnter(Collider other) {
        Debug.Log("Collision!");
            if (other.gameObject.layer == 10) {
                Debug.Log("Detected Player1 Object");
                MeshRenderer mesh = GetComponent<MeshRenderer>();
                if (alliance != 1) {

            }
            mesh.material = players[0];
        } if (other.gameObject.layer == 11) {
            MeshRenderer mesh = GetComponent<MeshRenderer>();
            mesh.material = players[1];
            if (alliance != 2) {

            }
        }
    }
    
	// Update is called once per frame
	void Update () {
		
	}
}
