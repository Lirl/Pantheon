using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {

    public int xTile = -1;
    public int yTile = -1;
    public int code = -1;
    public GameObject effect;
    public float m_Speed = 120f;

    private void OnTriggerEnter(Collider other) {
        var collided = other.GetComponent<Disk>();
        if (!collided) {
            return;
        }
        var alliance = collided.Alliance;
        //Create a plus
        if (code == 0) {
            CreatePlus(alliance, xTile, yTile);
        }
        if (code == 1) {
            collided.Enlarge();
            Destroy(gameObject);
        }
    }

    private void CreatePlus(int alliance, int xTile, int yTile) {
        for (int i = 0; i < Board.MAP_WIDTH; i++) {
            Board.Instance.SetTileAlliance(alliance, i, yTile);
        }
        for (int i = 0; i < Board.MAP_HEIGHT; i++) {
            Board.Instance.SetTileAlliance(alliance, xTile, i);
        }
        Instantiate(effect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void Update() {
        transform.Rotate(Vector3.up * Time.deltaTime * m_Speed);
    }
}

