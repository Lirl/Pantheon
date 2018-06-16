using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuyCard : MonoBehaviour {

    private User user;
    public int cost;
    public int code;
    public GameObject CantBuy;

    private void Start() {
        user = FindObjectOfType<User>();
        if (!user) {
            Debug.Log("Card cant find user");
        }
    }

    public void Buy() {
        if (user.gold >= cost) {
            user.disks.Add(code);
            user.gold -= cost;
            transform.parent = GameObject.Find("Deck").transform;
        }
    }
}
