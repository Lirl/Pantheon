using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void DiskReleaseHandler();

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
    public float HeightOffSet;

    public List<DiskReleaseHandler> OnDiskRelease = new List<DiskReleaseHandler>();

    public AudioManager AudioManager;

    LineRenderer line;
    public SpringJoint SJ;
    public MeshRenderer mesh;
    public List<string> Buffs = new List<string>();

    internal void AddBuff(string v) {
        Buffs.Add(v);
    }

    internal bool hasBuff(string v) {
        return Buffs.Contains(v);
    }

    internal void RemoveBuff(string v) {
        Buffs.Remove(v);
    }

    public int Alliance;

    public double Health;
    public double TotalHealth;

    public int Attack = 1;
    public int Id = -1;
    public enum ClassType { Rock, Paper, Scissors };
    public ClassType classType;

    public bool Enable = false; // when disabled, block any mouse interaction with this game object

    private static int _idCounter = 0;
    private float enlarge = 0;
    private float _normalScaleX;
    private float _normalScaleZ;
    private float _enlargeScaleX;
    private float _enlargeScaleZ;
    private float _shrinkScaleX;
    private float _shrinkScaleZ;

    public bool _released = false;
    private bool inField = false;
    private bool outOfBounds = false;

    public bool _forcedRelease = false;


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
        AudioManager = GameObject.FindObjectOfType<AudioManager>();
        if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("PunInit", PhotonTargets.All, alliance);
        }
        else {
            PunInit(alliance);
        }
    }

    [PunRPC]
    public void PunInit(int alliance) {
        GameObject hook;
        if (alliance == 1) {
            hook = GameObject.Find("HostHook");
        }
        else {
            hook = GameObject.Find("ClientHook");
        }

        GetComponent<SpringJoint>().connectedAnchor = hook.transform.position;
        GetComponent<SpringJoint>().connectedBody = hook.GetComponent<Rigidbody>();

        _normalScaleX = gameObject.transform.localScale.x;
        _normalScaleZ = gameObject.transform.localScale.z;
        originalPosition = transform.position;


        if (!Board.Instance.isYourTurn) {
            Destroy(GetComponent<SpringJoint>());
        }

        if (!Board.Instance.isHost && !Board.Instance.isTutorial) {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        Alliance = alliance;

        // Set disk color
        GetComponent<MeshRenderer>().material = Resources.Load("Materials/Color" + alliance, typeof(Material)) as Material;

        mesh = SJ.connectedBody.GetComponent<MeshRenderer>();
        line.SetPosition(0, SJ.connectedBody.position);

        // Health bar settings
        Debug.Log("Init " + name + " with health " + Health + " of total " + TotalHealth);
        
        HealthBar.maxValue = (float)TotalHealth;
        HealthBar.minValue = 0;

        HealthBar.value = (float)Health;

        Id = GenerateId();
        HeightOffSet = (Id == 3) ? 0.1f : 0;
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
            transform.position = new Vector3(transform.position.x, 0.1f + HeightOffSet, transform.position.z);
        }

        if (enlarge > 0) {
            if (gameObject.transform.localScale.x < _enlargeScaleX && gameObject.transform.localScale.z < _enlargeScaleZ) {
                gameObject.transform.position += new Vector3(0, 0.05f, 0);
                gameObject.transform.localScale += new Vector3(0.1f, 0, 0.1f);
            } else {
                enlarge = 0;
            }
        } else if (enlarge < 0) {
            if (gameObject.transform.localScale.x > _shrinkScaleX && gameObject.transform.localScale.z > _shrinkScaleZ) {
                gameObject.transform.position -= new Vector3(0, 0.05f, 0);
                gameObject.transform.localScale -= new Vector3(0.1f, 0, 0.1f);
            } else {
                enlarge = 0;
            }
        }

        if (inField && !outOfBounds) {
            CheckIfOutOfBounds();
        }
        if ((outOfBounds && gameObject.transform.localScale.x > 0.1) || (Health <= 0 && gameObject.transform.localScale.x > 0.1)) {
            gameObject.transform.localScale -= new Vector3(0.05f, 0, 0.05f);
        }
        if (gameObject.transform.localScale.x < 0.1) {
            DestroyDisk();
        }
    }

    private void CheckIfOutOfBounds() {
        if (Math.Abs(gameObject.transform.position.x) > 32 || Math.Abs(gameObject.transform.position.z) > 47) {
            outOfBounds = true;
            Board.Instance.OnDiskOutOfBound(this);
        }
    }

    private void OnMouseDown() {
        if (!Enable) {
            return;
        }

        _released = false;

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

        var pos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        transform.position = originalPosition;

        if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("Release", PhotonTargets.All, pos);
        }
        else {
            Release(pos);
        }
    }

    public void ReleaseOnTurnEnd() {
        if (PhotonNetwork.connected) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("PunReleaseOnTurnEnd", PhotonTargets.All);
        }
        else {
            PunReleaseOnTurnEnd();
        }
    }

    [PunRPC]
    public void PunReleaseOnTurnEnd() {
        Debug.Log("PunReleaseOnTurnEnd");
        if (!_released) {
            _forcedRelease = true;
            Release(Board.Instance.GetHook(this.Alliance).transform.position + new Vector3(0, 3f, 0));
        }
    }


    [PunRPC]
    public void Release(Vector3 pos) {
        Debug.Log("Release fired for player " + (Board.Instance.isHost ? 1 : 0) + " pos: " + transform.position.x + "," + transform.position.y + "," + transform.position.z);

        Enable = false;
        isMouseDown = false;

        _released = true;

        isMouseDown = false;
        Rigidbody.isKinematic = false;
        if (line) {
            Destroy(line);
        }

        transform.position = pos;
        StartCoroutine(UnHook());

        Invoke("RunOnDiskReleaseHandlers", 1);

        Board.Instance.OnDiskReleased(this);
        if (!_forcedRelease) {
            Invoke("StopMoving", 5);
        }
    }


    public void RunOnDiskReleaseHandlers() {
        foreach (var item in OnDiskRelease) {
            item.Invoke();
        }
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
        var powerUp = collision.gameObject.GetComponent<PowerUp>();
        if (!powerUp) {
            Debug.Log("Didnt find powerUp");
        }
        if (!disk) {
            AudioManager.Play("Wall Hit");
        }
        else {
            AudioManager.Play("Disk Hit");
        }


        if (!Board.Instance.isYourTurn || !disk) {
            return;
        }


        // Alliance is current player alliance
        if (disk.Alliance != Alliance) {
            if (classType == ClassType.Rock) {
                if (disk.classType == ClassType.Paper) {
                    disk.DealDamage(Attack * 0.5);
                }
                else if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage(Attack * 2);
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
                    disk.DealDamage(Attack * 0.5);
                }
                else {
                    disk.DealDamage(Attack * 2);
                }
            }
            else {
                if (disk.classType == ClassType.Scissors) {
                    disk.DealDamage(Attack);
                }
                else if (disk.classType == ClassType.Rock) {
                    disk.DealDamage(Attack * 0.5);
                }
                else {
                    disk.DealDamage(Attack * 2);
                }
            }
        }

        Board.Instance.OnDiskDamangeHandled(this, disk);
    }

    public void ForceSyncPosition() {
        if (Board.Instance.isYourTurn) {
            if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
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
        if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("PunDealDamage", PhotonTargets.All, dmg);
        }
        else {
            PunDealDamage(dmg);
        }
    }

    [PunRPC]
    private void PunDealDamage(double dmg) {
        Health = Health - dmg;
        HealthBar.value = (float)Health;
    }

    public void SetHealth(double health) {
        if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("PunSetHealth", PhotonTargets.All, health);
        } else {
            PunSetHealth(health);
        }
    }

    [PunRPC]
    public void PunSetHealth(double health) {
        Health = health;
        HealthBar.value = (float)Health;
    }

    internal void DestroyDisk() {
        if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            if (photonView.isMine) {
                PhotonNetwork.Destroy(photonView);
            }
        }
        else {
            // TODO: have a death effect
            Destroy(gameObject);
        }
    }


    internal void Enlarge(float ratio) {
        _enlargeScaleX = _normalScaleX * ratio;
        _enlargeScaleZ = _normalScaleZ * ratio;
        enlarge = 1;
    }


    internal void Shrink(float ratio) {
        _enlargeScaleX = gameObject.transform.localScale.x / ratio;
        _enlargeScaleZ = gameObject.transform.localScale.z / ratio;
        enlarge = -1;
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

    void OnDestroy() {
        Debug.Log("Disk " + Id + " destroyed");
    }

}
