using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disk : MonoBehaviour {

    private bool isMouseDown = false;
    private bool zoomOut = false;
    public Rigidbody Rigidbody;
    public SpringJoint SpringJoint;
    public float releaseTime = 0.15f;
    public float cameraAdjuster;
    public float endTurn;

    private void Start() {
        SpringJoint = GetComponent<SpringJoint>();
        
    }

    private void Update() {
        if (isMouseDown) {
            Rigidbody.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            ZoomInCamera();
        } 
        if (zoomOut) {
            ZoomOutCamera();
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
        yield return new WaitForSeconds(endTurn);
        zoomOut = true;

    }

    private void ZoomInCamera () {
        if (Camera.main.orthographicSize <= 60) {
            Camera.main.orthographicSize += cameraAdjuster;
        }
    }

    private void ZoomOutCamera () {
        if (Camera.main.orthographicSize >= 45) {
            Camera.main.orthographicSize -= cameraAdjuster;
        } else {
            zoomOut = false;
        }
    }
}
