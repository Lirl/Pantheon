using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Board : MonoBehaviour
{
    public static Board Instance { set; get; }
    public int MaxScore { get; private set; }
    public GameObject Hand;

    public double WinScoreThreshold = 0.8; // you need 80% control over the board

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

    private bool isZoomedOut = false;
    private bool isZoomedIn = false;

    public int TurnCounter = 0;

    private Vector3 boardOffset = new Vector3(29.0f, 0, 44.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0.125f, 0.5f);

    public bool isHost;

    internal void OnDiskReleased(Disk disk) {
        var pos = disk.gameObject.transform.position;
        client.Send("CRELEASEDISK|" + disk.Alliance + "|" + pos.x + "|" + pos.z);
    }

    private bool isYourTurn;
    private bool hasKilled;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public Text MessageBox;

    public GameObject[] CurrentCharacter = new GameObject[2];

    private Client client;
    

    private void Start()
    {
        Instance = this;
        client = FindObjectOfType<Client>();
        Hand = GameObject.Find("Hand");

        alertCanvas = GameObject.Find("MessageCanvas").GetComponent<CanvasGroup>();

        /*if (highlightsContainer) {
            foreach (Transform t in highlightsContainer.transform) {
                t.position = Vector3.down * 100;
            }
        }*/

        if (client)
        {
            isHost = client.isHost;
            //Alert(client.players[0].name + " versus " + client.players[1].name);
            Alert(isHost ? "I am Host" : "I am Client");
        }

        // Client player has its camera rotate 180 degrees
        if(!isHost) {
            Camera.main.transform.rotation = Quaternion.Euler(90, 180, 0);
        }

        // Host starts(Trust me dont change that)
        isYourTurn = !isHost;

        GenerateBoard();
        StartTurn();
    }

    public void StartTurn() {
        TurnCounter++;
        isYourTurn = !isYourTurn;
        Debug.Log("Player " + (isYourTurn ? 1 : 2) + " turn #" + TurnCounter);
        if (isYourTurn) {
            if (Hand) {
                Alert("Setting Hand ACTIVE for " + (isHost ? 1 : 2) + " turn #" + TurnCounter);
                Hand.SetActive(true);
            }
            isZoomedOut = true;
        } else {
            if (Hand) {
                Alert("Setting Hand NOT-ACTIVE for " + (isHost ? 1 : 2) + " turn #" + TurnCounter);
                Hand.SetActive(false);
            }
            isZoomedIn = true;
        }
    }

    public void CreateSelectedDisk() {
        client.Send("CCREATEDISK|" + (isHost ? 1 : 0));
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            
            //ShowMessage("Click x: " + mouseOver.x + " : " + mouseOver.y);

            SetTileAlliance((isHost ? 0 : 1), (int)mouseOver.x, (int)mouseOver.y);
        }

        if (isZoomedOut) {
            if (Camera.main.orthographicSize >= 45) {
                Camera.main.orthographicSize -= 0.5f;
            } else {
                isZoomedOut = false;
            }
        }

        if (isZoomedIn) {
            if (Camera.main.orthographicSize <= 75) {
                Camera.main.orthographicSize += 0.5f;
            } else {
                isZoomedIn = false;
            }
        }

        UpdateAlert();
        UpdateMouseOver();
    }

    internal void ReleaseDisk(int alliance, float x, float y) {
        // If the disk is yours, you already got that effect
        if (alliance != (isHost ? 1 : 0)) {
            // Alliance is of the opposing player
            // therefore we should play his move by moving his piece to according to his mouse position
            // and releasing
            GameObject disk = CurrentCharacter[alliance];
            if (!disk) {
                Debug.LogError("Release disk: Could not release disk of player " + alliance + " since its undefined");
            }
            disk.GetComponent<Disk>().SetPositionAndRelease(new Vector3(x, 0, y));
        } else {
            // This is the player that played the move
            // He should end the turn
            EndTurn();
        }

        
    }

    internal GameObject CreateDisk(int alliance) {
        GameObject hook;
        if(alliance == 1) {
            hook = GameObject.Find("HostHook");
        } else {
            hook = GameObject.Find("ClientHook");
        }

        var ins = Instantiate(DummyDisk, hook.transform.position, Quaternion.identity);
        ins.GetComponent<SpringJoint>().connectedAnchor = hook.transform.position;
        ins.GetComponent<SpringJoint>().connectedBody = hook.GetComponent<Rigidbody>();
        ins.GetComponent<Disk>().Init(alliance);
        CurrentCharacter[alliance] = ins;

        // Handle UI
        if (Hand) {
            Hand.SetActive(false);
        }

        isZoomedOut = true;

        return ins;
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

    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x + boardOffset.x);
            mouseOver.y = (int)((hit.point.z + -1 * boardOffset.z) * -1);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }
    /*private void UpdatePieceDrag(Piece p)
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            p.transform.position = hit.point + Vector3.up;
        }
    }*/


    IEnumerator EndTurnAfter() {
        yield return new WaitForSeconds(5);
        EndTurn();
    }

    private void EndTurn()
    {
        Debug.Log("EndTurn");
        var found = CheckVictory();
        Debug.Log("After looking for victor " + found);

        if (found) {
            return;
        }

        Debug.Log("Winner not found");
        client.Send("CSTARTTURN");
    }

    private void Victory(bool isWhite)
    {
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

    private void GenerateBoard()
    {
        // Reset score
        Score[0] = 0;
        Score[1] = 0;

        MaxScore = MAP_WIDTH * MAP_HEIGHT;

        // fill 2D Array with -1
        for (int row = 0; row < MAP_WIDTH; row++) {
            for (int column = 0; column < MAP_HEIGHT; column++) {
                Tiles[row, column] = -1;
            }
        }
    }

    public void SetTileAlliance(int alliance, int x, int y) {

        if(x == -1 || y == -1) {
            return;
        }

        if(Tiles[x, y] != -1) {
            Tiles[x, y] = alliance;
        }

        Tiles[x, y] = alliance;
        Score[alliance]++;

        Debug.Log("Tile " + x + "," + y + " was set with " + alliance);
    }

    public void Alert(string text)
    {
        // TODO: create text to display information
        alertCanvas.GetComponentInChildren<Text>().text = text;
        alertCanvas.alpha = 1;
        lastAlert = Time.time;
        alertActive = true;
    }
    public void UpdateAlert()
    {
        if (alertActive)
        {
            if (Time.time - lastAlert > 1.5f)
            {
                alertCanvas.alpha = 1 - ((Time.time - lastAlert) - 1.5f);

                if (Time.time - lastAlert > 2.5f)
                {
                    alertActive = false;
                }
            }
        }
    }

    private bool CheckVictory() {
        return false;
        // Something wrong in this code:
        for (int i = 0; i < Score.Length; i++) {
            if(Score[i]/MaxScore >= WinScoreThreshold) {
                client.Send("CPLAYERWON|" + i);
                return true;
            }
        }

        return true;
    }

    public void ChatMessage(string msg)
    {
        GameObject go = Instantiate(messagePrefab) as GameObject;
        go.transform.SetParent(chatMessageContainer);

        go.GetComponentInChildren<Text>().text = msg;
    }

    public void ShowMessage(String message) {
        Text i = GameObject.Find("MessageInput").GetComponent<Text>();
        i.text = message;
    }

    public void SendChatMessage()
    {
        InputField i = GameObject.Find("MessageInput").GetComponent<InputField>();

        if (i.text == "")
            return;

        client.Send("CMSG|" + i.text);

        i.text = "";
    }
}
