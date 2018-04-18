using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfromation : MonoBehaviour {

    public bool isOpen = false;

    public void OnMouseDown() {
        if (!isOpen) {
            isOpen = true;
            OpenInfo();
        }
    }

    private void OpenInfo() {
        
    }
}
