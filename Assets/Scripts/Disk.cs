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

    LineRenderer line;
    public SpringJoint SJ;
    public MeshRenderer mesh;

    public int Alliance;

    public bool Enable = true; // when disabled, block any mouse interaction with this game object

    private void Awake() {
        line = GetComponent<LineRenderer>();
        SJ = GetComponent<SpringJoint>();
        line.enabled = false;
    }

    public void Init(int alliance) {
        Alliance = alliance;
        mesh = SJ.connectedBody.GetComponent<MeshRenderer>();
        line.SetPosition(0, SJ.connectedBody.position);
    }

    public void Init(int alliance, bool enable) {
        Enable = enable;
        Init(alliance);
    }


    private void Update() {
        if (isMouseDown) {
            Rigidbody.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (line) {
                line.SetPosition(1, Rigidbody.position);
            }
            ZoomInCamera();
        } 
        if (zoomOut) {
            ZoomOutCamera();
        }
    }

    private void OnMouseDown() {
        if(!Enable) {
            return;
        }

        isMouseDown = true;
        Rigidbody.isKinematic = true;
        mesh.enabled = true;
        if(line) {
            line.enabled = true;
        }
    }

    private void OnMouseUp() {
        if (!Enable) {
            return;
        }

        isMouseDown = false;
        Board.Instance.OnDiskReleased(this);
        //Release();
    }

    public void Release() {
        isMouseDown = false;
        Rigidbody.isKinematic = false;
        if (line) {
            Destroy(line);
        }
        StartCoroutine(UnHook());
    }

    public void SetPositionAndRelease(Vector3 position) {
        Debug.Log("SetPositionAndRelease : " + position);
        transform.position = position;
        Release();
    }

    IEnumerator UnHook() {
        mesh.enabled = false;
        yield return new WaitForSeconds(releaseTime);
        Destroy(GetComponent<SpringJoint>());
        yield return new WaitForSeconds(endTurn);
        zoomOut = true;
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
