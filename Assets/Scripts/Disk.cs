using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Disk : MonoBehaviour {

    private bool isMouseDown = false;
    private bool zoomOut = false;
    public Rigidbody Rigidbody;
    public SpringJoint SJ;
    public float releaseTime = 0.15f;
    public float cameraAdjuster;
    public float endTurn;
    public int alliance;
    public GameObject Hand;
    MeshRenderer mesh;
    LineRenderer line;

    private void Awake() {
        line = GetComponent<LineRenderer>();
        SJ = GetComponent<SpringJoint>();
        line.enabled = false;
        mesh = SJ.connectedBody.GetComponent<MeshRenderer>();
        line.SetPosition(0, SJ.connectedBody.position);
    }

    private void Start() {
        
    }

    private void Update() {
        if (isMouseDown) { 
            Rigidbody.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if(line) {
                line.SetPosition(1, Rigidbody.position);
            }
            ZoomInCamera();
        } 
    }

    private void OnMouseDown() {
        isMouseDown = true;
        Rigidbody.isKinematic = true;
        mesh.enabled = true;
        line.enabled = true;
    }

    private void OnMouseUp() {
        isMouseDown = false;
        Board.Instance.SendDiskRelease(transform.position);
        Release();
    }

    public void Release() {
        Rigidbody.isKinematic = false;
        if (line) {
            Destroy(line);
        }
        StartCoroutine(UnHook());
    }

    public void SetPositionAndRelease(Vector3 position) {
        transform.position = position;
        Release();
    }

    IEnumerator UnHook() {
        mesh.enabled = false;
        yield return new WaitForSeconds(releaseTime);
        Destroy(GetComponent<SpringJoint>());
        yield return new WaitForSeconds(endTurn);
        zoomOut = true;
        if (Hand) {
            Hand.SetActive(true);
        }
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
