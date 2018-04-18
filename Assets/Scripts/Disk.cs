using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Disk : MonoBehaviour {

    private bool isMouseDown = false;
    private bool zoomOut = false;
    public Rigidbody Rigidbody;
    public SpringJoint SpringJoint;
    public float releaseTime = 0.15f;
    public float cameraAdjuster;
    public float endTurn;
    public int alliance;
    public GameObject hand;
    MeshRenderer mesh;
    LineRenderer line;

    private void Awake() {
        line = GetComponent<LineRenderer>();
        line.enabled = false;
        mesh = SpringJoint.connectedBody.GetComponent<MeshRenderer>();
        hand = GameObject.Find("Hand");
        SpringJoint = GetComponent<SpringJoint>();
        line.SetPosition(0, SpringJoint.connectedBody.position);
        if (hand == null) {
            Debug.Log("Couldnt find hand panel");
        }
    }

    private void Start() {
        
    }

    private void Update() {
        if (isMouseDown) {
            line.enabled = true;
            Rigidbody.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            line.SetPosition(1, Rigidbody.position);
            ZoomInCamera();
        } 
    }

    private void OnMouseDown() {
        isMouseDown = true;
        Rigidbody.isKinematic = true;
        mesh.enabled = true;
    }

    private void OnMouseUp() {
        isMouseDown = false;
        Rigidbody.isKinematic = false;
        Destroy(line);
        StartCoroutine(UnHook());
        
    }

    IEnumerator UnHook() {
        mesh.enabled = false;
        yield return new WaitForSeconds(releaseTime);
        Destroy(GetComponent<SpringJoint>());
        yield return new WaitForSeconds(endTurn);
        zoomOut = true;
        hand.SetActive(true);
    }

    private void ZoomInCamera () {
        if (Camera.main.orthographicSize <= 75) {
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
