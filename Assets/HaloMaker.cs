using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HaloMaker : MonoBehaviour {

    public Outline outline;
    bool isSizeMax = false;


    private void Start() {
            
    }

    // Update is called once per frame
    void Update () {
        /*
        if (outline.effectDistance.x < 9 && outline.effectDistance.x > 6) {
            Debug.Log("Increasing radius");
            outline.effectDistance += new Vector2(0.1f, 0.1f);
        } else if (outline.effectDistance.x > 9 && outline.effectDistance.x < 6) {
            Debug.Log("Decreasing radius");
            outline.effectDistance -= new Vector2(0.1f, 0.1f);
        }
        */
        Color color = outline.effectColor;
        
        outline.effectColor += new Color((color.r + 1f) % 255, (color.g + 1f) % 255, (color.b + 1f) % 255);
	}
}
