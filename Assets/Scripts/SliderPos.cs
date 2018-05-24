using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderPos : MonoBehaviour {

    public GameObject character;


    // Use this for initialization
    void Start() {
        var disk = character.GetComponent<Disk>();
        gameObject.transform.parent = GameObject.FindGameObjectWithTag("Canvas").transform;
    }

    // Update is called once per frame
    void Update() {
        if (character) {
            if (!Board.Instance.isHost) {
                transform.position = new Vector3(character.transform.position.x, character.transform.position.y, character.transform.position.z - 5);
            } else {
                transform.position = new Vector3(character.transform.position.x, character.transform.position.y, character.transform.position.z + 5f);
            }
        } else {
            if (gameObject) {
                Destroy(gameObject);
            }
        }        
    }
}