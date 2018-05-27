using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : Photon.PunBehaviour {

    public int xTile = -1;
    public int yTile = -1;
    public int code = -1;
    public GameObject effect;
    public float m_Speed = 120f;
    public AudioManager AudioManager;

    private void Start() {
        AudioManager = GameObject.FindObjectOfType<AudioManager>();
    }

    private void OnTriggerEnter(Collider other) {

        var collided = other.GetComponent<Disk>();
        if (!collided) {
            return;
        } else {
            AudioManager.PlayFor("Trail Renderer Sound", 0.05f);
        }
        if (!collided._released) {
            return;
        }
        Instantiate(effect, transform.position + new Vector3(0, 2, 0), Quaternion.identity);
        var alliance = collided.Alliance;
        //Create a plus
        if (code == 0) {
            CreatePlus(alliance, xTile, yTile);
        }
        if (code == 1) {
            collided.Enlarge();
        }

        if(PhotonNetwork.connected && PhotonNetwork.inRoom) {
            PhotonNetwork.Destroy(photonView);
        } else {
            Destroy(gameObject);
        }
        
    }

    private void CreatePlus(int alliance, int xTile, int yTile) {
        Debug.Log("Creating plus on " + xTile + "," + yTile + " for " + alliance);
        for (int i = 0; i < Board.MAP_WIDTH_REAL; i++) {
            Board.Instance.HandleSetTileAlliance(alliance, i, yTile);
        }
        for (int i = 0; i < Board.MAP_HEIGHT_REAL; i++) {
            Board.Instance.HandleSetTileAlliance(alliance, xTile, i);
        }
        Instantiate(effect, transform.position, Quaternion.identity);
        if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            PhotonNetwork.Destroy(photonView);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Update() {
        transform.Rotate(Vector3.up * Time.deltaTime * m_Speed);
    }
}

