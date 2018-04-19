using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Board : MonoBehaviour {
    public static Board Instance { set; get; }
    public const int MAP_WIDTH = 59;
    public const int MAP_HEIGHT = 89;

    public int[,] Tiles = new int[MAP_WIDTH, MAP_HEIGHT];
    public int[] Score = new int[2]; // Score[0] <= Host. Score[1] <= Client

    public Transform chatMessageContainer;
    public GameObject messagePrefab;

    public GameObject highlightsContainer;
    public GameObject DummyDisk;

    public CanvasGroup alertCanvas;
    private float lastAlert;
    private bool alertActive;
    private bool gameIsOver;
    private float winTime;

    private Vector3 boardOffset = new Vector3(29.0f, 0, 44.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0.125f, 0.5f);

    public bool isHost;
    private bool isHostTurn;
    private bool hasKilled;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public GameObject[] CurrentCharacter = new GameObject[2];

    private Client client;

    private void Start() {
        Instance = this;
        client = FindObjectOfType<Client>();

        alertCanvas = GameObject.Find("Canvas").GetComponent<CanvasGroup>();

        if (highlightsContainer) {
            foreach (Transform t in highlightsContainer.transform) {
                t.position = Vector3.down * 100;
            }
        }

        if (client) {
            isHost = client.isHost;
            Alert(client.players[0].name + " versus " + client.players[1].name);
        } else {
            // Informing player and enable UI
            /*Alert(client.players[0].name + " player's turn");
            Transform c = GameObject.Find("Canvas").transform;
            foreach (Transform t in c)
            {
                t.gameObject.SetActive(false);
            }

            c.GetChild(0).gameObject.SetActive(true);
            */

        }


        // Client player has its camera rotate 180 degrees
        if (!isHost) {
            Camera.main.transform.rotation = Quaternion.Euler(90, 180, 0);
        }

        isHostTurn = true;
        GenerateBoard();
    }

    public void SendDiskRelease(Vector3 position) {
        if (client) {
            client.Send("CRELEASEDISK|" + (isHost ? 1 : 0) + "|" + position.x + "|" + position.z);
        }
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            SetTileAlliance((isHost ? 0 : 1), (int)mouseOver.x, (int)mouseOver.y);
            // Your turn
            //if ((isHost && isHostTurn) || (!isHost && !isHostTurn)) {
            //client.Send("SETTILE|" + (isHost ? 0 : 1) + "|" + mouseOver.x + "|" + mouseOver.y);
            //}
        }

        if (gameIsOver) {
            if (Time.time - winTime > 3.0f) {
                Server server = FindObjectOfType<Server>();
                Client client = FindObjectOfType<Client>();

                if (server)
                    Destroy(server.gameObject);

                if (client)
                    Destroy(client.gameObject);

                SceneManager.LoadScene("Menu");
            }

            return;
        }

        UpdateAlert();
        UpdateMouseOver();

        if ((isHost) ? isHostTurn : !isHostTurn) {
            int x = (int)mouseOver.x;
            int y = (int)mouseOver.y;
        }
    }

    internal void ReleaseDisk(int alliance, float x, float y) {
        if (alliance == (isHost ? 1 : 0)) {
            return;
        }

        // Alliance is of the opposing player
        // therefore we should play his move by moving his piece to according to his mouse position
        // and releasing

        GameObject disk = CurrentCharacter[alliance];
        disk.GetComponent<Disk>().SetPositionAndRelease(new Vector3(x, 0, y));
    }

    internal GameObject CreateDisk(float x, float y) {
        return Instantiate(DummyDisk, new Vector3(x, 0, y) - boardOffset, Quaternion.identity);
    }

    internal GameObject CreateDisk(GameObject go, int x, int y) {
        return Instantiate(go, new Vector3(x, 0, y) - boardOffset, Quaternion.identity);
    }

    internal GameObject CreateDisk(GameObject go, Vector3 position) {
        Debug.Log("Creating disk " + position);
        var ins = Instantiate(go, position, Quaternion.identity);
        ins.GetComponent<SpringJoint>().connectedAnchor = new Vector3(0, 1.9f, 0);
        return ins;
    }

    private void UpdateMouseOver() {
        if (!Camera.main) {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("Board"))) {
            mouseOver.x = (int)(hit.point.x + boardOffset.x);
            mouseOver.y = (int)((hit.point.z + -1 * boardOffset.z) * -1);
        } else {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }

    private void EndTurn() {
        if (client) {
            string msg = "CMOV|";
            msg += startDrag.x.ToString() + "|";
            msg += startDrag.y.ToString() + "|";
            msg += endDrag.x.ToString() + "|";
            msg += endDrag.y.ToString();

            client.Send(msg);
        }

        isHostTurn = !isHostTurn;
        CheckVictory();
    }
    private void CheckVictory() {
        return;
        // TODO: create win condition
        /*var ps = FindObjectsOfType<Piece>();
        bool hasWhite = false, hasBlack = false;
        for (int i = 0; i < ps.Length; i++) {
            if (ps[i].isWhite)
                hasWhite = true;
            else
                hasBlack = true;
        }

        if (!hasWhite)
            Victory(false);
        if (!hasBlack)
            Victory(true);*/
    }
    private void Victory(bool isWhite) {
        winTime = Time.time;

        if (isWhite)
            Alert("White player has won!");
        else
            Alert("Black player has won!");

        gameIsOver = true;
    }

    /*private void Highlight()
    {
        foreach (Transform t in highlightsContainer.transform)
        {
            t.position = Vector3.down * 100;
        }

        if (forcedPieces.Count > 0)
            highlightsContainer.transform.GetChild(0).transform.position = forcedPieces[0].transform.position + Vector3.down * 0.1f;

        if (forcedPieces.Count > 1)
            highlightsContainer.transform.GetChild(1).transform.position = forcedPieces[1].transform.position + Vector3.down * 0.1f;
    }*/

    private void GenerateBoard() {
        // Reset score
        Score[0] = 0;
        Score[1] = 0;

        // fill 2D Array with -1
        for (int row = 0; row < MAP_WIDTH; row++) {
            for (int column = 0; column < MAP_HEIGHT; column++) {
                Tiles[row, column] = -1;
            }
        }
    }

    public void SetTileAlliance(int alliance, int x, int y) {

        if (x == -1 || y == -1) {
            return;
        }

        if (Tiles[x, y] != -1) {
            Tiles[x, y] = alliance;
        }

        Tiles[x, y] = alliance;
        Score[alliance]++;

        Debug.Log("Tile " + x + "," + y + " was set with " + alliance);
    }

    public void Alert(string text) {
        // TODO: create text to display information
        alertCanvas.GetComponentInChildren<Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        alertActive = true;
    }
    public void UpdateAlert() {
        if (alertActive) {
            if (Time.time - lastAlert > 1.5f) {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);

                if (Time.time - lastAlert > 2.5f) {
                    alertActive = false;
                }
            }
        }
    }

    public void ChatMessage(string msg) {
        GameObject go = Instantiate(messagePrefab) as GameObject;
        go.transform.SetParent(chatMessageContainer);

        go.GetComponentInChildren<Text>().text = msg;
    }

    public void ShowMessage(String message) {
        Text i = GameObject.Find("MessageInput").GetComponent<Text>();
        i.text = message;
    }

    public void SendChatMessage() {
        InputField i = GameObject.Find("MessageInput").GetComponent<InputField>();

        if (i.text == "")
            return;

        client.Send("CMSG|" + i.text);

        i.text = "";
    }
}
