using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class User : MonoBehaviour {

    public static User user;

    public string userName;
    public List<int> disks;
    public int wins;
    public int losses;

    void Awake () {
        if (user == null) {
            DontDestroyOnLoad(gameObject);
            user = this;
        } else if (user != this) {
            Destroy(gameObject);
        }   
	}

    public void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/userInfo.dat");

        UserData userData = new UserData {
            userName = this.userName,
            disks = this.disks,
            wins = this.wins,
            losses = this.losses
        };

        bf.Serialize(file, userData);
        file.Close();
    }

    public void Load() {
        if(File.Exists(Application.persistentDataPath + "/userInfo.dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/userInfo.dat", FileMode.Open);

            UserData userDate = (UserData)bf.Deserialize(file);
            disks = userDate.disks;
            userName = userDate.userName;
            wins = userDate.wins;
            losses = userDate.losses;

            file.Close();
        }

    }

    public void NewUser(string name) {
        user.userName = name;
        user.wins = 0;
        user.losses = 0;
        user.disks = new List<int>();
    }
}

[Serializable]
class UserData {
    public string userName;
    public List<int> disks;
    public int wins;
    public int losses;
}
