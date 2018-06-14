using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bloodlust : MonoBehaviour {

    // Use this for initialization
    private Disk self;
    private void Awake() {
        Init();
    }
    void Start () {
        Init();
    }

    public void Init() {
        self = GetComponent<Disk>();

        self.OnDiskRelease.Add(delegate () {
            var disks = Board.Instance.DisksList.FindAll(disk => disk && disk.gameObject && disk.Alliance == self.Alliance);
            for (int i = 0; i < disks.Count; i++) {
                disks[i].Enlarge(2);
                disks[i].AddBuff("Bloodlust");
            }
        });
    }

    private void OnDestroy() {
        var disks = Board.Instance.DisksList.FindAll(disk => disk && disk.gameObject && disk.Alliance == self.Alliance && disk.hasBuff("Bloodlust"));

        // Only disks of the same alliance that has bloodlust buff
        for (int i = 0; i < disks.Count; i++) {
            disks[i].Shrink(2);
            disks[i].RemoveBuff("Bloodlust");
        }
    }

}
