

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour {
    public Text Message;
    private bool stopped;
    private bool currentlyShowing;

    public static TutorialManager Instance { get; private set; }

    // Use this for initialization
    void Start() {
        Instance = this;
    }

    // Update is called once per frame
    void Update() {

    }

    public void ShowTutorialClassMessageInDelay() {

    }

    public void ShowMessage(string text, int time, int fontSize) {
        Message.fontSize = fontSize;
        Message.color = new Color(Message.color.r, Message.color.g, Message.color.b, 0);
        Message.text = text;
        StartCoroutine(FadeTextToFullAlpha(1f, Message));
        Invoke("FadeOut", time);
    }

    public void ShowMessageInSeconds(float delay, string text, int time, int fontSize) {
        StartCoroutine(ShowInDelay(delay, text, time, fontSize));
    }

    public IEnumerator ShowInDelay(float delay, string text, int time, int fontSize) {
        yield return new WaitForSeconds(delay);
        ShowMessage(text, time, fontSize);
    }

    public void ShowMessage(string text, int time) {
        ShowMessage(text, time, 25);
    }

    public void FadeOut() {
        StartCoroutine(FadeTextToZeroAlpha(1f, Message));
    }

    public IEnumerator FadeTextToFullAlpha(float t, Text i) {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
        while (i.color.a < 1.0f && !stopped) {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a + (Time.deltaTime / t));
            yield return null;
        }
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
    }

    public IEnumerator FadeTextToZeroAlpha(float t, Text i) {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f && !stopped) {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
        i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
    }
}
