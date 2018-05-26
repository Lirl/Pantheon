using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

     public void LoadMenu() {
        Application.LoadLevel("menu");
    }

    public void LoadMain() {
        //Application.LoadLevel("menu");
        Application.LoadLevel("Main");
    }


    public void LoadSceneByName(string scene) {
        Application.LoadLevel(scene);
    }


    //Used in CreateUser scene only! do not attach to any other scene
    public void LoadMenuAfterCreatingUser() {
        var texts = FindObjectsOfType<Text>();
        string name = texts[0].text;
        if (name == "") {
            Debug.Log("No Name");
        } else {
            //May be changed to LoadSceneByName later
            LoadMain();
        }
    }
}
