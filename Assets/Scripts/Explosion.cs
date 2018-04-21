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
    Disk disk;


    // Use this for initialization
    void Start() {
        disk = GetComponent<Disk>();
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

        Collider[] toBlast = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider c in toBlast) {
            Cube rb = c.GetComponent<Cube>();
            if (rb) {
                //Debug.Log(c.name);
                rb.SetAlliance(disk.Alliance);
            }
        }
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
    }
}