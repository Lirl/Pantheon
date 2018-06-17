using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Photon.PunBehaviour {

    public static Dictionary<string, string> Effects = new Dictionary<string, string>();
    bool _initialized = false;

    public static EffectManager Instance { get; private set; }

    // Use this for initialization
    void Start () {
        Init();
	}

    void Awake() {
        Init();
    }

    public void Init() {
        if(_initialized) {
            return;
        }

        _initialized = true;
        Instance = this;
        InitEffects();
    }

    public void InitEffects() {
        Effects.Add("Bloodlust", "Buffs/DarkAura");
        Effects.Add("BloodlustCaster", "Buffs/DarkAuraCaster");
        Effects.Add("MinorHeal", "Heal/MinorHeal");
        Effects.Add("MageCast", "Cast/MageCast");
    }

    public GameObject PlayEffect(string name, Vector3 position, GameObject parent) {

        Debug.Log("Playing effect " + name);

        // Only the player that plays this turn triggers the effect
        // though effect is seen in both players screen via PunPlayEffect
        if (!Board.Instance.isYourTurn) {
            return null;
        }

        GameObject effect = null;
        if (Effects.ContainsKey(name)) {
            if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
                photonView.RPC("PunPlayEffect", PhotonTargets.All, name, position, parent);
            } else {
                PunPlayEffect(name, position, parent);
            }
        } else {
            Debug.LogWarning("Effect " + name + " was not initialized in InitEffect");
        }

        return effect;
    }

    [PunRPC]
    public GameObject PunPlayEffect(string name, Vector3 position, GameObject parent) {
        Debug.Log("Actual Playing effect " + name);
        GameObject effect = null;
        var prefab = Resources.Load("Effects/" + Effects[name]);
        effect = (GameObject)Instantiate(prefab, position + new Vector3(0, 5, 0), Quaternion.identity);

        if(effect) {
            Debug.Log("Effect " + name + " has been created successfuly");
        }

        if (effect && parent) {
            effect.transform.SetParent(parent.transform);
        }
        
        return effect;
    }

    public void PlayHitEffect(Vector3 position) {
        if (PhotonNetwork.connected && PhotonNetwork.inRoom) {
            photonView.RPC("PunPlayHitEffect", PhotonTargets.All, position);
        } else {
            PunPlayHitEffect(position);
        }
	}

    [PunRPC]
    private void PunPlayHitEffect(Vector3 position) {
        var prefab = Resources.Load("Effects/Hit/Hit3");
        Instantiate(prefab, position + new Vector3(0, 5, 0), Quaternion.identity);
    }
}
