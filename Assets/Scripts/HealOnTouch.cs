using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealOnTouch : MonoBehaviour {

    public double HealAmount = 1;

    private void OnCollisionEnter(Collision collision) {
        Debug.Log("HealOnTouch triggered");
        var disk = collision.gameObject.GetComponent<Disk>();
        var powerUp = collision.gameObject.GetComponent<PowerUp>();
        var self = GetComponent<Disk>();

        if (!Board.Instance.isYourTurn || !disk) {
            return;
        }

        Debug.Log("HealOnTouch isMyTurn");
        // Alliance is current player alliance
        if (disk.Alliance == self.Alliance) {
            Debug.Log("HealOnTouch Same Alliance");
            // Where -1 is to full health, where 0 and positive heal that amout
            disk.SetHealth((HealAmount == -1) ? disk.TotalHealth : Math.Min(HealAmount + disk.Health, disk.TotalHealth));
            EffectManager.Instance.PlayEffect("MinorHeal", disk.gameObject.transform.position, disk.gameObject);
        }
    }

}
