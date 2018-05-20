using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UseCard : MonoBehaviour {

    public int code;
    public GameObject card;
    public CollectionManager cm;

    public TextMeshProUGUI use;

    public void Use() {
        cm.Swap(this);
    }
}
