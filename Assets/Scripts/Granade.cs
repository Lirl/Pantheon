using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Granade : MonoBehaviour {

    public float delay = 3f;
    public float radius = 10f;
    public float explosionForce = 5000f;
    float countdown;
    bool stopChecking = false;
    bool hasExploaded = false;
    public GameObject explosionEffect;
    Disk disk;


    // Use this for initialization
	void Start () {
        disk = GetComponent<Disk>();
	}
	
	// Update is called once per frame
	void Update () {
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
        Instantiate(explosionEffect, transform.position, transform.rotation);
        
        Collider[] toBlast =  Physics.OverlapSphere(transform.position, radius);
        
        foreach (Collider c in toBlast) {
            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb != null && rb.gameObject.layer == 9) {
                rb.AddExplosionForce(explosionForce, transform.position, radius);
                rb.AddForce(transform.position.x - rb.transform.position.x, 0f, transform.position.z - rb.transform.position.z);
            }
        }
        Destroy(gameObject);
    }
}
