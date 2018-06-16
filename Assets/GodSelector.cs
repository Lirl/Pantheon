using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodSelector : MonoBehaviour {

    private User user;
    public int code;

	// Use this for initialization
	void Start () {
        user = GameObject.FindObjectOfType<User>();
    }
    
    public void SetGod() {
        user.god = code;
        user.Save();
    }
}
