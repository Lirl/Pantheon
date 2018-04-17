using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disk : MonoBehaviour {

    private bool isMouseDown = false;
    public Rigidbody Rigidbody;
    public SpringJoint SpringJoint;
    public float releaseTime = 0.15f;

    private void Start() {
        SpringJoint = GetComponent<SpringJoint>();
        
    }

    private void Update() {
        if (isMouseDown) {
            Rigidbody.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseDown() {
        isMouseDown = true;
        Rigidbody.isKinematic = true;
    }

    private void OnMouseUp() {
        isMouseDown = false;
        Rigidbody.isKinematic = false;
        StartCoroutine(UnHook());
    }

    IEnumerator UnHook() {
        yield return new WaitForSeconds(releaseTime);
        Destroy(GetComponent<SpringJoint>());
    }
}
