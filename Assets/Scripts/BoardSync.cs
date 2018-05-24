using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class BoardSync : Photon.PunBehaviour {

    Board _board;
    bool _initialized = false;

    private void Awake() {
        Init();
    }

    public void Start() {
        Init();
    }

    public void Init() {
        if (_initialized) {
            return;
        }

        _initialized = true;
        Debug.Log("Attempting to get Board script");
        _board = GameObject.Find("BaseField").GetComponent<Board>();

        if (_board) {
            Debug.Log("board found :)");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        Debug.Log("Board Sync OnPhotonSerializeView " + stream.isWriting);
        if (stream.isWriting) {
            stream.SendNext(_board.GetTilesAsString());
            //stream.SendNext(_board.Score); 
        } else {

            _board.HandleSyncTiles((string)stream.ReceiveNext());
            //_board.Score = ((int[])stream.ReceiveNext());
            
        }
    }
}
