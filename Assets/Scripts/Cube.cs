using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour {
    MeshRenderer mesh;
    public Material[] materials = new Material[6];
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
        if (alliance == -1) {
            alliance = 0; // default material
        } else {
            if (alliance == 0) {
                alliance = alliance + 2;
            } else {
                alliance = alliance + 3;
            }

        }

        // 0 - normal grey
        // 1 - dark grey
        // 2 - red normal
        // 3 - red dark
        // 4 - blue normal
        // 5 - blue dark

        mesh.material = materials[alliance + ((X + Y) % 2 == 0 ? 1 : 0)];
    }

    public override string ToString() {
        return X + "," + Y + "=" + Alliance;
    }


    private void OnTriggerEnter(Collider other) {
        // check if this is not your turn.
        // collider is handled only in the player who has the turn
        if(!Board.Instance.isYourTurn) {
            return;
        }

        var disk = other.gameObject.GetComponent<Disk>();

        if (disk) {

            Board.Instance.HandleSetTileAlliance(disk.Alliance, X, Y);
        }
    }
}
