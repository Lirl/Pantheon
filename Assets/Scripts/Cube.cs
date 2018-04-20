using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {
    MeshRenderer mesh;
    public Material[] materials = new Material[3];
    public int Alliance;
    public int X;
    public int Y;

    void Awake() {
        mesh = GetComponent<MeshRenderer>();
    }

    void Start() {
        mesh = GetComponent<MeshRenderer>();
    }

    public void Init(int alliance, int x, int y) {
        this.X = x;
        this.Y = y;
        SetAlliance(alliance);
    }

    public void SetAlliance(int alliance) {
        Alliance = alliance;
        if(alliance == -1) {
            alliance = 2; // default material
        }
        mesh.material = materials[alliance];

        if((X % 6 == 0) && (Y % 6 == 0)) {
            mesh.material.color = new Color(mesh.material.color.r + 1.0f, mesh.material.color.r + 1.0f, mesh.material.color.r + 1.0f);
        }
        
    }


    private void OnTriggerEnter(Collider other) {
        
        var disk = other.gameObject.GetComponent<Disk>();

        if (disk) {
            Debug.Log("Cube collision triggered");
            Debug.Log("Change cube alliance to " + disk.Alliance);
            SetAlliance(disk.Alliance);
        }
    }
}
