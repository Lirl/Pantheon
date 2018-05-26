using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePillars : Photon.PunBehaviour {

    public bool reachedFarSide = true;
    public bool reachedCloseSide = false;
    public float offset = 0.1f;
    public float zPos;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if ((Board.Instance.isHost || Board.Instance.isTutorial) && Board.Instance.isPillarsMoving) {
            if (transform.position.z <= zPos && !reachedFarSide) {
                transform.position += new Vector3(0, 0, offset * Time.deltaTime);
            }
            if (transform.position.z >= zPos && !reachedFarSide) {
                reachedFarSide = true;
                reachedCloseSide = false;
            }
            if (transform.position.z >= -zPos && !reachedCloseSide) {
                transform.position += new Vector3(0, 0, -offset * Time.deltaTime);
            }
            if (transform.position.z <= -zPos && !reachedCloseSide) {
                reachedCloseSide = true;
                reachedFarSide = false;
            }
        }
    }

    public void MoveUp() {
        offset *= -1;
    }
}