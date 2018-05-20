using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour {

    public User user;
    public GameObject Deck;
    public GameObject Cards;
    public UseCard lastClickedCard;

    public GameObject[] Descriptions;
    public GameObject alreadyInUse;

    public bool firstCardClicked = false;
    public bool firstClickedIsOnDeck;

    private void Start() {        
        user = GameObject.FindObjectOfType<User>();    
        for (int i = 0; i < user.deck.Count; i++) {
            var card = GameObject.Find("" + user.deck[i]);
            card.transform.parent = Deck.transform;
        }
    }


    public void PutIn(UseCard card) {
        card.card.transform.parent = Deck.transform;
    }


    public void Swap(UseCard card) {
        //Pressing The First Card
        if (!firstCardClicked) {
            ChangeText("Replace");
            firstCardClicked = true;
            lastClickedCard = card;
            firstClickedIsOnDeck = (card.card.transform.parent == Deck.transform);
            Glow(true);
            Debug.Log("First card clicked is in " + card.card.transform.parent);
        //Choosing which card to replace
        } else {
            ChangeText("Use");
            Glow(false);
            if (!firstClickedIsOnDeck) {
                user.deck.Remove(card.code);
                user.deck.Add(lastClickedCard.code);
                user.disks.Remove(lastClickedCard.code);
                user.disks.Add(card.code);
                card.card.transform.parent = Cards.transform;
                lastClickedCard.card.transform.parent = Deck.transform;
                firstCardClicked = false;
            } else {
                user.deck.Add(card.code);
                user.deck.Remove(lastClickedCard.code);
                user.disks.Add(lastClickedCard.code);
                user.disks.Remove(card.code);
                card.card.transform.parent = Deck.transform;
                lastClickedCard.card.transform.parent = Cards.transform;
                firstCardClicked = false;
            }
        }
    }

    private void ChangeText(string v) {
        foreach (GameObject go in Descriptions) {
            var useCard = go.GetComponentInChildren<UseCard>();
            useCard.use.text = v;
        }
    }

    private void Glow(bool glow) {
        var section = (firstClickedIsOnDeck == true) ? Cards : Deck;
        var children = section.GetComponentsInChildren<Outline>();
        for (int i = 0; i < children.Length; i++) {
            children[i].enabled = glow;
        }
    }

    public void InsertCard(UseCard card) {
        if (user.deck.Count < 6) {
            PutIn(card);
        } else {
            Swap(card);
        }
    }
}
