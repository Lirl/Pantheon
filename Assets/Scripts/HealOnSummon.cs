using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealOnSummon : MonoBehaviour {
    public float HealAmount = 2;
    // Use this for initialization
    private Disk self;
    private void Awake() {
        Init();
    }
    void Start() {
        Init();
    }

    public void Init() {
        self = GetComponent<Disk>();

        self.OnDiskRelease.Add(delegate () {
            var disks = Board.Instance.DisksList.FindAll(disk => disk && disk.gameObject && disk.Alliance == self.Alliance);
            for (int i = 0; i < disks.Count; i++) {
                var disk = disks[i];
                if (disk != self) {
                    disk.SetHealth((HealAmount == -1) ? disk.TotalHealth : Math.Min(HealAmount + disk.Health, disk.TotalHealth));
                    EffectManager.Instance.PlayEffect("MinorHeal", disk.gameObject.transform.position, disk.gameObject);
                }
            }
        });
    }
}
