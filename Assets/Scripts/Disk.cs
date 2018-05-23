using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Disk : Photon.PunBehaviour {
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
    public double Health = -1;

    public int Attack = 1;
    public int Id = -1;
    public enum ClassType { Rock, Paper, Scissors };
    public ClassType classType;

    public bool Enable = false; // when disabled, block any mouse interaction with this game object

    private static int _idCounter = 0;
    private bool enlarge = false;
    private bool _released = false;

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
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("PunInit", PhotonTargets.All, alliance);
    }

    [PunRPC]
    public void PunInit(int alliance) {
        GameObject hook;
        if (alliance == 1) {
            hook = GameObject.Find("HostHook");
        } else {
            hook = GameObject.Find("ClientHook");
        }

        GetComponent<SpringJoint>().connectedAnchor = hook.transform.position;
        GetComponent<SpringJoint>().connectedBody = hook.GetComponent<Rigidbody>();

        if(!Board.Instance.isYourTurn) {
            Destroy(GetComponent<SpringJoint>());
        }

        Debug.Log("PunInit : " + alliance);
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

        if (_released) {
            transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
        }

        if (enlarge) {
            if (gameObject.transform.localScale.x < 13 && gameObject.transform.localScale.z < 13) {
                gameObject.transform.position += new Vector3(0, 0.05f, 0);
                gameObject.transform.localScale += new Vector3(0.1f, 0, 0.1f);
            }
            else {
                enlarge = false;
            }
        }
    }

    private void OnMouseDown() {
        if (!Enable) {
            return;
        }

        _released = false;

        GetComponent<PhotonTransformView>();

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

        Enable = false;

        isMouseDown = false;
        var pos = transform.position;
        transform.position = originalPosition;

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("Release", PhotonTargets.All, pos);

        //Release(pos);
    }

    [PunRPC]
    public void Release(Vector3 pos, PhotonMessageInfo info) {
        Debug.Log("Release fired for player " + (Board.Instance.isHost ? 1 : 0) + " pos: " + transform.position.x + "," + transform.position.y + "," + transform.position.z);

        _released = true;

        isMouseDown = false;
        Rigidbody.isKinematic = false;
        if (line) {
            Destroy(line);
        }

        transform.position = pos;
        StartCoroutine(UnHook());

        Board.Instance.OnDiskReleased(this);
        Invoke("StopMoving", 5);
    }

    public void StopMoving() {
        Rigidbody.velocity = Vector3.zero;
    }

    IEnumerator UnHook() {
        mesh.enabled = false;
        yield return new WaitForSeconds(releaseTime);
        if (GetComponent<SpringJoint>()) {
            Destroy(GetComponent<SpringJoint>());
        }
        startedMoving = true;
        yield return new WaitForSeconds(endTurn);
    }


    private void OnCollisionEnter(Collision collision) {
        var disk = collision.gameObject.GetComponent<Disk>();
        if (!Board.Instance.isYourTurn || !disk || disk.Alliance == (Board.Instance.isHost ? 1 : 0)) return;
        if ((Board.Instance.isYourTurn) && (disk.Alliance != Alliance)) {
            if (classType == ClassType.Rock) {
                if (disk.classType == ClassType.Paper) {
                    disk.DealDamage((int)(Attack * 0.5));
                }
                else if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage((int)(Attack * 2));
                }
                else {
                    disk.DealDamage(Attack);
                }
            }
            else if (classType == ClassType.Paper) {
                if (disk.classType == ClassType.Paper) {
                    disk.DealDamage(Attack);
                }
                else if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage((double)(Attack * 0.5));
                }
                else {
                    disk.DealDamage((double)(Attack * 2));
                }
            }
            else {
                if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage(Attack);
                }
                else if (disk.classType == ClassType.Rock) {
                    disk.DealDamage((double)(Attack * 0.5));
                }
                else {
                    disk.DealDamage((double)(Attack * 2));
                }
            }
        }
    }

    private void DealDamage(double dmg) {
        Health = Health - dmg;
        if (Health < 0) {
            PhotonNetwork.Destroy(photonView);
        }
    }

    internal void DestroyDisk() {
        PhotonNetwork.Destroy(photonView);
    }


    internal void Enlarge() {
        enlarge = true;
    }

    public override string ToString() {
        var pos = transform.position;
        return pos.x + "," + pos.z + "=" + Id;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        Debug.Log("Board Sync OnPhotonSerializeView " + stream.isWriting);
        if (stream.isWriting) {
            stream.SendNext(Alliance);
        }
        else {
            Alliance = (int)stream.ReceiveNext();
        }
    }
}
