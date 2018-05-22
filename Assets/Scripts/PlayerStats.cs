using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour {

    public int games;
    public int wins;
    public int gold;

    public TextMeshProUGUI Games;
    public TextMeshProUGUI Wins;
    public TextMeshProUGUI Gold;
    public TextMeshProUGUI Name;

    // Use this for initialization
    void Start() {
        var player = FindObjectOfType<User>();
        wins = player.wins;
        games = player.wins + player.losses;
        gold = player.gold;
        Games.text = "Games: " + games.ToString();
        Wins.text = "Wins: " + wins.ToString();
        Gold.text = "Gold: " + gold.ToString();
        Name.text = player.userName;
    }

    // Update is called once per frame
    void Update() {

    }
}
