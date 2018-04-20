using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Disk : MonoBehaviour {
    private Vector3 originalPosition;
    internal bool startedMoving = false;
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
        }
    }

    private void OnMouseDown() {
        if (!Enable) {
            return;
        }

        originalPosition = transform.position;

        isMouseDown = true;
        Rigidbody.isKinematic = true;
        mesh.enabled = true;
        if (line) {
            line.enabled = true;
        }

        Board.Instance.OnDiskClick(this);
    }

    private void OnMouseUp() {
        if (!Enable) {
            return;
        }

        isMouseDown = false;
        var pos = transform.position;
        transform.position = originalPosition;
        Board.Instance.OnDiskReleased(this, pos);
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
