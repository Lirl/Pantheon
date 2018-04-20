using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;

public class Board : MonoBehaviour {
    public static Board Instance { set; get; }
    public int MaxScore { get; private set; }
    public GameObject Hand;

    public double WinScoreThreshold = 0.8; // you need 80% control over the board

    public const int MAP_WIDTH = 59;
    public const int MAP_HEIGHT = 89;

    public GameObject[,] Tiles = new GameObject[MAP_WIDTH, MAP_HEIGHT];
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
    public List<int> Deck;

    private Vector3 boardOffset = new Vector3(29.0f, 0, 44.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0.125f, 0.5f);

    public bool isHost;
    public Dictionary<int, Disk> Disks = new Dictionary<int, Disk>();

    internal void SaveDisk(int id, Disk disk) {
        Disks.Add(id, disk);
    }

    internal void OnDiskReleased(Disk disk) {
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if(client) {
            client.Send("CRELEASEDISK|" + disk.Alliance + "|" + pos.x + "|" + pos.z);
        } else {
            ReleaseDisk(disk.Alliance, pos.x, pos.z);
        }
    }

    private bool isYourTurn;
    private bool hasKilled;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public Text MessageBox;

    public List<CardInformation> CardTypes = new List<CardInformation>();
    public GameObject[] CurrentCharacter = new GameObject[2];

    private Client client;


    private void Start() {
        Instance = this;
        client = FindObjectOfType<Client>();
        Hand = GameObject.Find("Hand");

        alertCanvas = GameObject.Find("MessageCanvas").GetComponent<CanvasGroup>();

        /*if (highlightsContainer) {
            foreach (Transform t in highlightsContainer.transform) {
                t.position = Vector3.down * 100;
            }
        }*/

        if (client) {
            isHost = client.isHost;
            //Alert(client.players[0].name + " versus " + client.players[1].name);
            Alert(isHost ? "I am Host" : "I am Client");
        } else {
            isHost = true;
        }

        // Client player has its camera rotate 180 degrees
        if (!isHost) {
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
            // Add a new card to player hand (current turn)
            DrawCard();

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

    internal void HandleDestroyDisk(int id) {
        var disk = Disks[id];
        if (disk) {
            disk.DestroyDisk();
        }
    }

    internal void DestroyDisk(Disk disk) {
        client.Send("CDISTROYDISK|" + disk.Id);
    }

    internal bool isActiveDisk(GameObject gameObject) {
        throw new NotImplementedException();
    }

    private void DrawCard() {
        if (Deck.Count == 0) {
            Debug.LogError("Deck is empty");
            return;
        }

        int code = Deck[0];
        Deck.RemoveAt(0);

        Card.CreateCard(code, Hand.transform);
    }

    public void CreateSelectedDisk() {
        GameObject button = EventSystem.current.currentSelectedGameObject;
        Card card = button.GetComponent<Card>();
        int code = card.Code; // Indicates which disk should be created according ot Board DiskTypes array

        button.transform.parent = null;
        Destroy(button);
        if(client) {
            client.Send("CCREATEDISK|" + (isHost ? 1 : 0) + "|" + code);
        } else {
            CreateDisk((isHost ? 1 : 0), code);
        }
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {

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

        // Alliance is of the opposing player
        // therefore we should play his move by moving his piece to according to his mouse position
        // and releasing
        GameObject disk = CurrentCharacter[alliance];
        if (!disk) {
            Debug.LogError("Release disk: Could not release disk of player " + alliance + " since its undefined");
        }

        // Actually releasing the disk
        disk.GetComponent<Disk>().SetPositionAndRelease(new Vector3(x, 0.1f, y));

        if (alliance == (isHost ? 1 : 0)) {
            // This is the player that played the move
            // He should end the turn
            EndTurn();
        }
    }

    internal GameObject CreateDisk(int alliance, int code) {
        GameObject hook;
        if (alliance == 1) {
            hook = GameObject.Find("HostHook");
        } else {
            hook = GameObject.Find("ClientHook");
        }

        var prefab = Resources.Load("Characters/Character" + code) as GameObject;

        var ins = Instantiate(prefab, hook.transform.position, Quaternion.identity);
        ins.GetComponent<SpringJoint>().connectedAnchor = hook.transform.position;
        ins.GetComponent<SpringJoint>().connectedBody = hook.GetComponent<Rigidbody>();
        ins.GetComponent<Disk>().Init(alliance, isYourTurn);
        CurrentCharacter[alliance] = ins;

        // Handle UI
        if (Hand) {
            Hand.SetActive(false);
        }

        isZoomedOut = true;

        return ins;
    }


    internal GameObject CreateDisk(int alliance) {
        GameObject hook;
        if (alliance == 1) {
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

    private void EndTurn() {
        Debug.Log("EndTurn");
        var found = CheckVictory();
        Debug.Log("After looking for victor " + found);

        if (found) {
            return;
        }

        Debug.Log("Winner not found");
        client.Send("CSTARTTURN");
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
        // Create deck list
        Deck = new List<int>();
        for (int i = 0; i < 30; i++) {
            // Set deck currently with values from 0 to 1
            Deck.Add(UnityEngine.Random.Range(0, 1));
        }

        // Reset score
        Score[0] = 0;
        Score[1] = 0;

        MaxScore = MAP_WIDTH * MAP_HEIGHT;

        GameObject go = Resources.Load("Cube") as GameObject;
        GameObject cubeWall = Resources.Load("CubeWall") as GameObject;
        GameObject waterCube = Resources.Load("WaterCube") as GameObject;
        /*
        // fill 2D Array with -1
        for (int row = 0; row < MAP_WIDTH; row++) {
            for (int column = 0; column < MAP_HEIGHT; column++) {
                Tiles[row, column] = Instantiate(go, new Vector3(row, -0.4f, column) - boardOffset, Quaternion.identity);
                Tiles[row, column].GetComponent<Cube>().Init(-1, row, column); // might be redundent as this is default
            }
        }*/

        // fill 2D Array with -1
        for (int row = -1; row < MAP_WIDTH + 1; row++) {
            for (int column = -1; column < MAP_HEIGHT + 1; column++) {
                if (row % 3 == 0 && column % 3 == 0) {
                    Tiles[row, column] = Instantiate(go, new Vector3(row, -0.4f, column) - boardOffset, Quaternion.identity);
                    //Tiles[row, column].transform.localScale = new Vector3(3, 3);
                    Tiles[row, column].GetComponent<Cube>().Init(-1, row, column); // might be redundent as this is default
                }//row == -1 || row == MAP_WIDTH ||
                if (row == -1 || row == MAP_WIDTH) {
                    if (column < (float)(MAP_HEIGHT / 3) || column > (float)((2.0f / 3.0f) * MAP_HEIGHT)) {
                        Instantiate(cubeWall, new Vector3(row, 0.5f, column) - boardOffset, Quaternion.identity);
                    } else {
                        Instantiate(waterCube, new Vector3(row, -0.4f, column) - boardOffset, Quaternion.identity);
                    }
                }
            }
        }
    }

    public void SetTileAlliance(int alliance, int x, int y) {

        if (x == -1 || y == -1) {
            return;
        }

        if (Tiles[x, y]) {
            Tiles[x, y].GetComponent<Cube>().SetAlliance(alliance);
        }

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

    private bool CheckVictory() {
        return false;
        // Something wrong in this code:
        for (int i = 0; i < Score.Length; i++) {
            if (Score[i] / MaxScore >= WinScoreThreshold) {
                client.Send("CPLAYERWON|" + i);
                return true;
            }
        }

        return true;
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
