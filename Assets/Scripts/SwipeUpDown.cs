using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeUpDown : MonoBehaviour {
    public float Value = 1.0f;
    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + (Mathf.PingPong(Time.time, Value) - (Value / 2)));
    }
}
