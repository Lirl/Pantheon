using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {

    public Animator anim;

    private void Start() {
        anim = GetComponent<Animator>();
    }

    private void ChangeScene() {
        GameManager.Instance.StartGame();
        anim.enabled = false;

    }
}
