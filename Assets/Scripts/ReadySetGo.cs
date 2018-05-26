using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadySetGo : MonoBehaviour {

    string[] shoutout = new string[] { "Set", "Go!" };
    public Text text;
    private int index;
    int textSize;
    private bool flag = true;

    // Use this for initialization
    void Start () {
        index = 0;
    }
	
	// Update is called once per frame
	void Update () {
        // Values between 0 and 1
        if(text.fontSize <= 24) {
            text.fontSize += 1;
        } 
        
        if(text.fontSize > 24 && flag) {
            
            flag = false;
            Invoke("ChangeText", 1);
        }
	}


    public void ChangeText() { 
        if(index == shoutout.Length) {
            text.text = "";
            Board.Instance.StartGame();
        }

        text.text = shoutout[index];
        text.fontSize = 15;
        index++;
        flag = true;
    }
}
