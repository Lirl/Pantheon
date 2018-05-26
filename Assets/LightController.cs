using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour {

    public Light light;

	// Update is called once per frame
	void Update () {
		if (Board.Instance.isLightsOn && light.intensity > 0.5) {
            light.intensity -= 0.01f;
        }
	}
}
