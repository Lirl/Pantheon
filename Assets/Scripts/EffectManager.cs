using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour {

    public static Dictionary<string, string> Effects = new Dictionary<string, string>();
    bool _initialized = false;

    // Use this for initialization
    void Start() {
        Init();
    }

    void Awake() {
        Init();
    }

    public void Init() {
        if (_initialized) {
            return;
        }

        _initialized = true;

        InitEffects();
    }

    public void InitEffects() {
        Effects.Add("Bloodlust", "Buffs/DarkAura");
        Effects.Add("BloodlustCaster", "Buffs/DarkAuraCaster");
        Effects.Add("MinorHeal", "Heal/MinorHeal");
    }

    public static GameObject PlayEffect(string name, Vector3 position, GameObject parent) {
        var ins = PlayEffect(name, position);
        if (parent) {
            ins.transform.SetParent(parent.transform);
        }
        return ins;
    }

    public static GameObject PlayEffect(string name, Vector3 position) {
        GameObject effect = null;
        if (Effects.ContainsKey(name)) {
            /*if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
                effect = PhotonNetwork.Instantiate("Effects/" + Effects[name], position + new Vector3(0 ,5 ,0), Quaternion.identity, 0);
            } else {*/
            var prefab = Resources.Load("Effects/" + Effects[name]);
            effect = (GameObject)Instantiate(prefab, position + new Vector3(0, 5, 0), Quaternion.identity);

        }
        else {
            Debug.LogWarning("Effect " + name + " was not initialized in InitEffect");
        }

        return effect;
    }


    public static void PlayHitEffect(Vector3 position) {

        /*if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            PhotonNetwork.Instantiate("Effects/Hit" + UnityEngine.Random.Range(1,3), position + new Vector3(0, 5, 0), Quaternion.identity, 0);
        } else {*/
        var prefab = Resources.Load("Effects/Hit/Hit" + UnityEngine.Random.Range(1, 3));
        Instantiate(prefab, position + new Vector3(0, 5, 0), Quaternion.identity);


    }
}
