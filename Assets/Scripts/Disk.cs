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
    public Slider HealthBar;

    LineRenderer line;
    public SpringJoint SJ;
    public MeshRenderer mesh;

    public int Alliance;
    public double Health = -1;
    public double TotalHealth = -1;

    public int Attack = 1;
    public int Id = -1;
    public enum ClassType { Rock, Paper, Scissors };
    public ClassType classType;

    public bool Enable = false; // when disabled, block any mouse interaction with this game object

    private static int _idCounter = 0;
    private bool enlarge = false;
    private bool _released = false;
    private bool inField = false;
    private bool outOfBounds = false;

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
        if (PhotonNetwork.connected) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("PunInit", PhotonTargets.All, alliance);
        } else {
            PunInit(alliance);
        }
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

        if (!Board.Instance.isYourTurn) {
            Destroy(GetComponent<SpringJoint>());
        }

        if(!Board.Instance.isHost && !Board.Instance.isTutorial) {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        
        Alliance = alliance;

        // Set disk color
        GetComponent<MeshRenderer>().material = Resources.Load("Materials/Color" + alliance, typeof(Material)) as Material;

        mesh = SJ.connectedBody.GetComponent<MeshRenderer>();
        line.SetPosition(0, SJ.connectedBody.position);
        if (Health == -1) {
            Health = 3;
            TotalHealth = Health;
            HealthBar.value = (float)Health;
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
            } else {
                enlarge = false;
            }
        }

        if (inField && !outOfBounds) {
            CheckIfOutOfBounds();
        }
        if (outOfBounds && gameObject.transform.localScale.x > 0.1) {
            gameObject.transform.localScale -= new Vector3(0.05f, 0, 0.05f);
        }
        if (gameObject.transform.localScale.x < 0.1 && Board.Instance.isYourTurn) {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void CheckIfOutOfBounds() {
        if (Math.Abs(gameObject.transform.position.x) > 32 || Math.Abs(gameObject.transform.position.z) > 47) {
            outOfBounds = true;
        }
    }

    private void OnMouseDown() {
        if (!Enable) {
            return;
        }

        _released = false;

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

        if (PhotonNetwork.connected) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("Release", PhotonTargets.All, pos);
        } else {
            Release(pos);
        }
    }

    [PunRPC]
    public void Release(Vector3 pos) {
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
        Invoke("SetInField", 1f);
        if (GetComponent<SpringJoint>()) {
            Destroy(GetComponent<SpringJoint>());
        }
        startedMoving = true;
        yield return new WaitForSeconds(endTurn);
    }

    public void SetInField() {
        Debug.Log("Disk is in the field");
        inField = true;
    }


    private void OnCollisionEnter(Collision collision) {

        var disk = collision.gameObject.GetComponent<Disk>();
        if (!Board.Instance.isYourTurn || !disk) {
            return;
        }

        // If this is your oppponent disk, return
        // We only handle current player disks disks of the opposite color
        if(disk.Alliance == Board.Instance.CurrentTurnAlliance()) {
            return;
        }


        Debug.Log("Current Turn Alliance: " + Board.Instance.CurrentTurnAlliance());
        Debug.Log("Collision " + Id + " Player Alliance : " + (Board.Instance.isHost ? 1 : 0) + ", isYourTurn: " + Board.Instance.isYourTurn);
        Debug.Log("ATK-ID: " + Id + " ,TARGET-ID: " + disk.Id);

        // Disk is the enemy

        // Alliance is current player alliance
        if (disk.Alliance != Alliance) {
            if (classType == ClassType.Rock) {
                if (disk.classType == ClassType.Paper) {
                    disk.DealDamage(Attack * 0.5);
                } else if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage(Attack * 2);
                } else {
                    disk.DealDamage(Attack);
                }
            } else if (classType == ClassType.Paper) {
                if (disk.classType == ClassType.Paper) {
                    disk.DealDamage(Attack);
                } else if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage(Attack * 0.5);
                } else {
                    disk.DealDamage(Attack * 2);
                }
            } else {
                if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage(Attack);
                } else if (disk.classType == ClassType.Rock) {
                    disk.DealDamage(Attack * 0.5);
                } else {
                    disk.DealDamage(Attack * 2);
                }
            }
        }
    }

    public void ForceSyncPosition() {
        if (Board.Instance.isYourTurn) {
            if (PhotonNetwork.connected) {
                PhotonView photonView = PhotonView.Get(this);
                photonView.RPC("PunForceSyncPosition", PhotonTargets.All, transform.position);
            }
        }
    }

    [PunRPC]
    public void PunForceSyncPosition(Vector3 pos, PhotonMessageInfo info) {
        transform.position = pos;
    }

    private void DealDamage(double dmg) {
        if (PhotonNetwork.connected) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("PunDealDamage", PhotonTargets.All, dmg);
        } else {
            PunDealDamage(dmg);
        }
    }

    [PunRPC]
    private void PunDealDamage(double dmg) {

        Health = Health - dmg;
        HealthBar.value = (float)Health;

        if (Health < 0 && Board.Instance.isYourTurn) {
            PhotonNetwork.Destroy(photonView);
        }
    }

    internal void DestroyDisk() {
        if (PhotonNetwork.connected) {
            PhotonNetwork.Destroy(photonView);
        } else {
            // TODO: have a death effect
            Destroy(gameObject);
        }
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
        } else {
            Alliance = (int)stream.ReceiveNext();
        }
    }

    void OnDestroy() {
        Debug.Log("Disk " + Id + " destroyed");
    }

}
