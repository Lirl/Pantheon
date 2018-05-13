using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class CheckIfFirstPlay : MonoBehaviour {

    public User user;

	// Use this for initialization
	void Awake () {
        if (!File.Exists(Application.persistentDataPath + "/userInfo.dat")) {
            SceneManager.LoadScene("CreateUser");
        }
        else {
            user.Load();
        }
        
    }

    private void Start() {
    }
}
