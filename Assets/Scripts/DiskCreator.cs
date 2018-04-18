using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DiskCreator : MonoBehaviour {

    GameObject Character;
    GameObject[] Hooks;
    GameObject handCanvas;
    SpringJoint CharacterSpringJoint;
    public int playerId;
    private bool isZoomedOut = false;

    public void CreateDisk (string disk) {
        Hooks = GameObject.FindGameObjectsWithTag("Hook");
        Character = (GameObject) Resources.Load(disk);
        handCanvas = GameObject.FindGameObjectWithTag("Hand1");
        CharacterSpringJoint = Character.GetComponent<SpringJoint>();
        CharacterSpringJoint.connectedBody = Hooks[playerId].GetComponent<Rigidbody>();
        Character.transform.position = Hooks[playerId].transform.position;
        Instantiate(Character);
        handCanvas.SetActive(false);
        isZoomedOut = true;
        
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (isZoomedOut) {
            if (Camera.main.orthographicSize >= 45) {
                Camera.main.orthographicSize -= 0.5f;
            } else {
                isZoomedOut = false;
            }
        }
	}
}
