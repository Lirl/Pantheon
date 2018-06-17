using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReadySetGo : MonoBehaviour {

    string[] shoutout = new string[] { "Set", "Go!" };
    public TextMeshProUGUI text;
    private int index;
    int textSize;
    private bool flag = true;
    private bool stop = false;

    // Use this for initialization
    void Start() {
        index = 0;
    }

    // Update is called once per frame
    void Update() {
        // Values between 0 and 1
        if (stop) {
            return;
        }

        if (text.fontSize <= 80) {
            text.fontSize += 5f;
        }

        if (text.fontSize > 80 && flag) {

            flag = false;
            Invoke("ChangeText", 1);
        }
    }


    public void ChangeText() {
        if (index == shoutout.Length) {
            text.text = "";
            Board.Instance.StartGame();
            stop = true;
            return;
        }

        text.text = shoutout[index];
        text.fontSize = 15;
        index++;
        flag = true;
    }
}
