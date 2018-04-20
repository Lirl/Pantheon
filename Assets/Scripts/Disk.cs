using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Disk : MonoBehaviour {

    private bool isMouseDown = false;
    private bool zoomOut = false;
    internal bool startedMoving = false; 
    public Rigidbody Rigidbody;
    public SpringJoint SpringJoint;
    public float releaseTime = 0.15f;
    public float cameraAdjuster;
    public float endTurn;

    LineRenderer line;
    public SpringJoint SJ;
    public MeshRenderer mesh;

    public int Alliance;
    public int Health = -1;
    public int Attack = 1;
    public int Id = -1;

    public bool Enable = true; // when disabled, block any mouse interaction with this game object

    private static int _idCounter = 0;

    public static int GenerateId() {
        _idCounter++;
        return _idCounter;
    }

    private void Awake() {
        line = GetComponent<LineRenderer>();
        SJ = GetComponent<SpringJoint>();
        line.enabled = false;
    }

    public void Init(int alliance) {
        Alliance = alliance;
        mesh = SJ.connectedBody.GetComponent<MeshRenderer>();
        line.SetPosition(0, SJ.connectedBody.position);
        if (Health == -1) {
            Health = 1;
        }
        Id = GenerateId();

        Board.Instance.SaveDisk(Id, this);
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
        if (!Enable) {
            return;
        }

        isMouseDown = true;
        Rigidbody.isKinematic = true;
        mesh.enabled = true;
        if (line) {
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
        startedMoving = true;
        yield return new WaitForSeconds(endTurn);
        zoomOut = true;
    }

    private void ZoomInCamera() {
        if (Camera.main.orthographicSize <= 75) {
            Camera.main.orthographicSize += cameraAdjuster;
        }
    }

    private void ZoomOutCamera() {
        if (Camera.main.orthographicSize >= 45) {
            Camera.main.orthographicSize -= cameraAdjuster;
        } else {
            zoomOut = false;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.name == "WaterCube") {
            var disk = collision.gameObject.GetComponent<Disk>();
            if (disk) {
                Board.Instance.DestroyDisk(disk);
            }
        }
    }

    private void DealDamage(int attack) {
        Health -= attack;
        if (Health < 0) {

        }
    }

    internal void DestroyDisk() {
        Destroy(this);
    }
}
