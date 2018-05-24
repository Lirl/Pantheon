using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour {

    public float delay = 3f;
    public float radius = 10f;
    float countdown;
    bool stopChecking = false;
    bool hasExploaded = false;
    public GameObject explosionEffect;
    SphereCollider sphereRadius;
    Disk disk;


    // Use this for initialization
    void Start() {
        disk = GetComponent<Disk>();
        sphereRadius = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update() {
        if (disk.startedMoving && !stopChecking) {
            countdown = delay;
            stopChecking = true;
        }
        if (stopChecking && !hasExploaded) {
            countdown -= Time.deltaTime;
            if (countdown <= 0) {
                Explode();
                hasExploaded = true;
            }
        }
    }

    private void Explode() {
        if(!Board.Instance.isYourTurn) {
            return;
        }

        Collider[] toBlast = Physics.OverlapSphere(transform.position, radius + (gameObject.transform.localScale.x / 6));

        foreach (Collider c in toBlast) {
            Cube cube = c.GetComponent<Cube>();
            if (cube) {
                //Debug.Log(c.name);
                Board.Instance.HandleSetTileAlliance(disk.Alliance, cube.X, cube.Y);
            }
        }
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
    }
}