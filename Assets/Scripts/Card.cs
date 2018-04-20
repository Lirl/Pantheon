using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour {
    public int Code;
    public string Name;
    public string Description;
    public string PrefabName;

    public static GameObject CreateCard(int code, Transform parent) {
        var ui = Resources.Load("Cards/Card" + code);
        var ins = Instantiate(ui, new Vector2(), Quaternion.identity) as GameObject;
        if(parent) {
            ins.transform.parent = parent;
        }

        return ins;
    }
}