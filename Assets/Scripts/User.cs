using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;

public class User : MonoBehaviour {

    public static User instance;

    public string userName;
    public List<int> disks;
    public int wins;
    public int losses;

    void Awake () {
        Debug.Log(Application.persistentDataPath);
        if (instance == null) {
            DontDestroyOnLoad(gameObject);
            instance = this;
            this.Load();
        } else if (instance != this) {
            Destroy(gameObject);
        }   
	}

    public void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/userInfo.dat");

        UserData userData = new UserData();
        userData.userName = instance.userName;
        userData.disks = instance.disks;
        userData.wins = instance.wins;
        userData.losses = instance.losses;

        Debug.Log("Saved");
        bf.Serialize(file, userData);
        file.Close();
    }

    public void Load() {
        if(File.Exists(Application.persistentDataPath + "/userInfo.dat")) {
            Debug.Log("File Exists");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/userInfo.dat", FileMode.Open);

            UserData userDate = (UserData)bf.Deserialize(file);
            Debug.Log(userDate.userName);
            disks = userDate.disks;
            userName = userDate.userName;
            wins = userDate.wins;
            losses = userDate.losses;

            file.Close();
        }

    }

    public void NewUser() {
        //Debug.LogError(Application.persistentDataPath);
        var texts = FindObjectsOfType<Text>();
        string newName = texts[0].text;

        Debug.Log(newName);
        instance.userName = newName;
        instance.disks = new List<int>();
        instance.wins = 0;
        instance.losses = 0;
        Save();
    }
}

[Serializable]
class UserData {
    public string userName;
    public List<int> disks;
    public int wins;
    public int losses;
}
