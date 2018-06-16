using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderPull : MonoBehaviour {

    public float delay = 1f;
    public float radius =909999990f;
    public float pullForce = 60f;
    List<LineRenderer> Lines = new List<LineRenderer>();
    Disk self;
    bool _pull;

    // Use this for initialization
    void Start() {
        Init();
    }

    void Awake() {
        Init();
    }

    public void Init() {
        self = GetComponent<Disk>();
        Invoke("CreateLines", delay);
    }

    private void CreateLines() {
        Debug.Log("CreateLines");
        /* var enemies = new List<Disk>();
        for (int i = 0; i < Board.Instance.DisksList.Count; i++) {
            var disk = Board.Instance.DisksList[i];
            Debug.Log(Vector3.Distance(disk.transform.position, self.transform.position));
            if (disk.Id != self.Id && (Vector3.Distance(disk.transform.position, self.transform.position) < radius)) {
                enemies.Add(disk);
            }
        }*/

        var enemies = Board.Instance.DisksList;
        for (int i = 0; i < enemies.Count; i++) {
            if(!enemies[i]) {
                continue;
            }
            Vector3 aim = (enemies[i].transform.position - self.transform.position).normalized;
            
            var line = enemies[i].gameObject.GetComponent<LineRenderer>();
            if (!line) {
                Debug.Log("Spider : line not found for enemy " + enemies[i].name);
                line = enemies[i].gameObject.AddComponent<LineRenderer>();
            }

            line.material = Resources.Load("Materials/ColorWhite", typeof(Material)) as Material;
            line.startColor = new Color(255, 255, 255, 255f);
            line.endColor = new Color(255, 255, 255, 255f);
            line.startWidth = 0.5f;
            line.endWidth = 0.7f;

            Lines.Add(line);
        }

        Debug.Log("Lines Count " + Lines.Count);
        Invoke("Pull", 0.5f);
    }

    public void FixedUpdate() {
        if(!_pull) {
            return;
        }

        for(int i = 0; i < Lines.Count; i++) {
            if(Lines[i]) {
                var enemy = Lines[i].gameObject;
                Vector3 aim = (enemy.transform.position - self.transform.position).normalized;
                enemy.gameObject.GetComponent<Rigidbody>().AddForce(aim * pullForce * -1);
            }
        }
    }

    public void Pull() {
        
        _pull = true;
        Invoke("RemoveLines", 0.5f);
    }

    public void RemoveLines() {
        _pull = false;
        Debug.Log("RemoveLines");
        for (int i = 0; i < Lines.Count; i++) {
            Destroy(Lines[i]);
        }
    }

    void Update() {
        for (int i = 0; i < Lines.Count; i++) {
            var line = Lines[i];
            if (line) {
                line.SetPosition(0, self.transform.position);
                line.SetPosition(1, line.gameObject.transform.position);
            }
        }
    }

    private void OnDestroy() {
        for(int i = 0; i < Lines.Count; i++) {
            Destroy(Lines[i]);
        }
    }
}
