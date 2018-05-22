using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using System.Text;
using Photon;

public class Board : Photon.PunBehaviour {
    public static Board Instance { set; get; }
    public int MaxScore { get; private set; }
    public GameObject Hand;

    public GameObject TimeMessage { get; private set; }

    public double WinScoreThreshold = 0.8; // you need 80% control over the board

    public const int MAP_WIDTH = 60;
    public const int MAP_HEIGHT = 90;

    public const int MAP_WIDTH_REAL = 20;
    public const int MAP_HEIGHT_REAL = 30;

    public int powerUpsAmount = 2;
    public GameObject[,] Tiles = new GameObject[MAP_WIDTH_REAL, MAP_HEIGHT_REAL];
    public int[] Score = new int[2]; // Score[0] <= Host. Score[1] <= Client

    public Transform chatMessageContainer;
    public GameObject messagePrefab;

    public GameObject highlightsContainer;
    public GameObject DummyDisk;

    public CanvasGroup alertCanvas;
    private GameObject yourScore;
    private GameObject opponentScore;
    private float lastAlert;
    private bool alertActive;
    private bool gameIsOver;
    private float winTime;
    public float gameTime = 10f;

    private bool isZoomedOut = false;
    private bool isZoomedIn = false;

    public int TurnCounter = 1;
    public List<int> Deck;

    public Vector3 boardOffset = new Vector3(29.0f, 0, 44.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0.125f, 0.5f);

    public bool isHost;
    public Dictionary<int, Disk> Disks = new Dictionary<int, Disk>();
    public GameObject WinMessage;
    public GameObject LoseMessage;
    public GameObject BackToMenu;

    internal void SaveDisk(int id, Disk disk) {
        Disks.Add(id, disk);
    }

    internal void OnDiskReleased(Disk disk, Vector3 pos) {
        ReleaseDisk(disk.Alliance, pos.x, pos.z);
    }

    public bool isYourTurn;

    public bool TurnHasEnded { get; private set; }

    private bool hasKilled;

    private Vector2 mouseOver;
    private Vector2 startDrag;
    private Vector2 endDrag;

    public Text MessageBox;

    public List<CardInformation> CardTypes = new List<CardInformation>();
    public GameObject[] CurrentCharacter = new GameObject[2];

    private Client client;
    private Vector3 prevDiskIdleResult = new Vector3(-1, -1, -1);

    public string SyncTilesString;

    private void Start() {
        Instance = this;
        client = FindObjectOfType<Client>();
        Hand = GameObject.Find("Hand");
        TimeMessage = GameObject.Find("TimeMessage") as GameObject;

        alertCanvas = GameObject.Find("AlertText").GetComponent<CanvasGroup>();
        yourScore = GameObject.Find("YourScore");
        opponentScore = GameObject.Find("OpponentScore");

        WinMessage.SetActive(false);
        LoseMessage.SetActive(false);
        BackToMenu.SetActive(false);

        /*if (highlightsContainer) {
            foreach (Transform t in highlightsContainer.transform) {
                t.position = Vector3.down * 100;
            }
        }*/

        if (PhotonNetwork.connected) {
            isHost = GameManager.Instance.isHost;
            //Alert(client.players[0].name + " versus " + client.players[1].name);
        }
        else {
            isHost = true;
        }

        Alert(isHost ? "I am Host" : "I am Client");

        yourScore.GetComponentInChildren<Text>().color = isHost ? Color.blue : Color.red;
        yourScore.GetComponentInChildren<Text>().text = "0";
        opponentScore.GetComponentInChildren<Text>().color = isHost ? Color.red : Color.blue;
        opponentScore.GetComponentInChildren<Text>().text = "0";

        // Client player has its camera rotate 180 degrees
        if (!isHost) {
            Camera.main.transform.rotation = Quaternion.Euler(90, 180, 0);
        }

        GenerateBoard();

        // Disable hand
        if (Hand) {
            Hand.SetActive(false);
        }

        if (isHost) {
            // Host starts
            StartTurn();
            Invoke("CreatePowerUp", 1f);
            Invoke("CheckWinner", gameTime);
        }
    }


    internal void HandleShowWinner(int alliance) {
        Debug.Log("HandleShowWinner started");
        if (Hand) {
            Hand.SetActive(false);
        }

        if (alliance == (isHost ? 1 : 0)) {
            WinMessage.SetActive(true);
        }
        else {
            LoseMessage.SetActive(true);
        }
        BackToMenu.SetActive(true);
    }

    public void OnDisksIdleTrigger() {

        var pos = new Vector3(0, 0, 0);
        foreach (var pair in Disks) {
            if (pair.Value) {
                pos += pair.Value.transform.position;
            }
        }

        var thresh = pos - prevDiskIdleResult;

        // Meaning disks stopped moving
        if (Math.Abs(thresh.x + thresh.z) < 1) {
            //Debug.Log("Disks stopped moving ! thresh " + (thresh.x + thresh.z));
            OnDisksIdle();
        }
        else {
            prevDiskIdleResult = pos;
            //Debug.Log("OnDisksIdleTrigger Invoke Started " + (thresh.x + thresh.z));
            Invoke("OnDisksIdleTrigger", 1);
        }
    }

    public void OnDisksIdle() {
        Debug.Log("OnDisksIdle");
        if (isYourTurn) {
            EndTurn();
        }
    }

    public void CheckWinner() {
        Debug.Log("CheckWinner");
        if (Score[0] >= Score[1]) {
            HandleShowWinner(0);
        }
        else {
            HandleShowWinner(1);
        }

        gameIsOver = true;
    }

    internal void OnDiskClick(Disk disk) {
        isZoomedIn = false;
        isZoomedOut = true;
    }

    public void StartTurn() {
        if (gameIsOver) {
            return;
        }

        isYourTurn = true;
        TurnHasEnded = false;

        Alert("Player " + (isHost ? 1 : 2) + " turn #" + TurnCounter + " YourTurn: " + isYourTurn);
        Debug.Log("Player " + (isHost ? 1 : 2) + " turn #" + TurnCounter + " YourTurn: " + isYourTurn);

        Alert("Player " + (isHost ? 1 : 2) + " turn #" + TurnCounter);
        // Add a new card to player hand (current turn)
        DrawCard();

        // 10 seconds turn
        //Invoke("EndTurn", 10);

        if (Hand) {
            Hand.SetActive(true);
        }

        isZoomedOut = true;
    }

    internal void HandleDestroyDisk(int id) {
        var disk = Disks[id];
        if (disk) {
            disk.DestroyDisk();
        }
    }

    internal void DestroyDisk(Disk disk) {
        HandleDestroyDisk(disk.Id);
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

        isZoomedOut = false;
        isZoomedIn = true;

        button.transform.parent = null;
        Destroy(button);

        if (Hand) {
            Hand.SetActive(false);
        }

        CreateDisk((isHost ? 1 : 0), code);
    }

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {

            //ShowMessage("Click x: " + mouseOver.x + " : " + mouseOver.y);

            //SetTileAlliance((isHost ? 0 : 1), (int)mouseOver.x, (int)mouseOver.y);
        }

        gameTime -= Time.deltaTime;
        if (gameTime >= 0 && !gameIsOver) {
            TimeMessage.GetComponentInChildren<Text>().text = Math.Floor(gameTime).ToString();
        }

        yourScore.GetComponentInChildren<Text>().text = Score[isHost ? 1 : 0].ToString();
        opponentScore.GetComponentInChildren<Text>().text = Score[isHost ? 0 : 1].ToString();

        if (isZoomedIn) {
            if (Camera.main.orthographicSize >= 45) {
                Camera.main.orthographicSize -= 0.5f;
            }
            else {
                isZoomedIn = false;
            }
        }

        if (isZoomedOut) {
            if (Camera.main.orthographicSize <= 75) {
                Camera.main.orthographicSize += 0.5f;
            }
            else {
                isZoomedOut = false;
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
            //Invoke("EndTurn", 2);
            Invoke("OnDisksIdleTrigger", 1);
        }
    }
    internal void CreatePowerUp() {
        return;
        int x = UnityEngine.Random.Range(1, MAP_WIDTH_REAL - 1);
        int y = UnityEngine.Random.Range(1, MAP_HEIGHT_REAL - 1);
        int amount = UnityEngine.Random.Range(0, powerUpsAmount);

        HandleCreatePowerUp(amount, x, y);

    }

    internal GameObject HandleCreatePowerUp(int code, int x, int y) {
        var toInstantiate = Resources.Load("Characters/PowerUps" + code) as GameObject;
        if (Tiles[x, y] && toInstantiate) {
            GameObject rune = PhotonNetwork.Instantiate("Characters/PowerUps" + code, Tiles[x, y].transform.position + new Vector3(0, 2f, 0), Quaternion.Euler(new Vector3(45, 45, 45)), 0);
            var runeScript = rune.GetComponent<PowerUp>();
            runeScript.code = code;
            runeScript.xTile = x;
            runeScript.yTile = y;

            if (isHost) {
                Invoke("CreatePowerUp", 10f);
            }

            //Instantiate(powerUp[UnityEngine.Random.Range(0, powerUp.Length)], location + new Vector3(0, 1.4f,0), Quaternion.identity);
            return rune;
        }
        return null;
    }

    internal GameObject CreateDisk(int alliance, int code) {
        Debug.Log("CreateDisk excepted. alliance = " + alliance + " code = " + code);
        GameObject hook;
        if (alliance == 1) {
            hook = GameObject.Find("HostHook");
        }
        else {
            hook = GameObject.Find("ClientHook");
        }

        Debug.Log("Attempting to load prefab " + "Characters/Character" + code + " for alliance " + alliance + " isYourTurn " + isYourTurn);
        var prefab = Resources.Load("Characters/Character" + code) as GameObject;
        var offset = (code == 3 ? 7f : 3f);
        var ins = PhotonNetwork.Instantiate("Characters/Character" + code, hook.transform.position + new Vector3(0, 3f, 0), Quaternion.identity, 0);

        ins.GetComponent<SpringJoint>().connectedBody = hook.GetComponent<Rigidbody>();
        ins.GetComponent<Disk>().Init(alliance, isYourTurn);
        CurrentCharacter[alliance] = ins;

        // Handle UI
        if (Hand) {
            Hand.SetActive(false);
        }

        return ins;
    }

    internal GameObject CreateDisk(int alliance) {
        GameObject hook;
        if (alliance == 1) {
            hook = GameObject.Find("HostHook");
        }
        else {
            hook = GameObject.Find("ClientHook");
        }

        var ins = PhotonNetwork.Instantiate(DummyDisk.name, hook.transform.position, Quaternion.identity, 0);
        ins.GetComponent<SpringJoint>().connectedAnchor = hook.transform.position;
        ins.GetComponent<SpringJoint>().connectedBody = hook.GetComponent<Rigidbody>();
        ins.GetComponent<Disk>().Init(alliance);
        CurrentCharacter[alliance] = ins;

        // Handle UI
        if (Hand) {
            Hand.SetActive(false);
        }

        return ins;
    }

    internal GameObject CreateDisk(float x, float y) {
        return PhotonNetwork.Instantiate(DummyDisk.name, new Vector3(x, 0, y) - boardOffset, Quaternion.identity, 0);
    }
    internal GameObject CreateDisk(GameObject go, int x, int y) {
        return PhotonNetwork.Instantiate(go.name, new Vector3(x, 0, y) - boardOffset, Quaternion.identity, 0);
    }
    internal GameObject CreateDisk(GameObject go, Vector3 position) {
        Debug.Log("Creating disk " + position);
        var ins = PhotonNetwork.Instantiate(go.name, position, Quaternion.identity, 0);
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
        }
        else {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }

    private void EndTurn() {
        if (isYourTurn && !TurnHasEnded) {
            TurnHasEnded = true;

            Debug.Log("EndTurn");

            // That's all you need to do for switching the turn
            // BoardSync will sync that data, and trigger SetTurn on the other player.
            // By doing so, the other player will trigger StartTurn, that will continue game loop as normal
            TurnCounter++;
            PhotonNetwork.RaiseEvent(0, GetTilesAsString(), true, null);
            isYourTurn = false;

            Debug.Log("TurnCounter is now : " + TurnCounter);

            // StartTurn(); start turn is invoked after a sync
            if (!PhotonNetwork.connected) {
                StartTurn();
            }

            if (Hand) {
                Hand.SetActive(false);
            }
        }
    }

    private void Victory(bool isWhite) {
        winTime = Time.time;

        if (isWhite)
            Alert("White player has won!");
        else
            Alert("Black player has won!");

        gameIsOver = true;
    }

    private void GenerateBoard() {
        // Create deck list
        Deck = new List<int>();
        for (int i = 0; i < 30; i++) {
            // Set deck currently with values from 0 to 1
            Deck.Add(0); // UnityEngine.Random.Range(0, 4));
        }

        for (int i = 0; i < 2; i++) {
            DrawCard();
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
        for (int row = 0; row < MAP_WIDTH; row++) {
            for (int column = 0; column < MAP_HEIGHT; column++) {
                if (row % 3 == 0 && column % 3 == 0) {
                    var ins = Instantiate(go, new Vector3(row, -0.4f, column) - boardOffset, Quaternion.identity);
                    //Tiles[row, column].transform.localScale = new Vector3(3, 3);
                    ins.GetComponent<Cube>().Init(-1, row / 3, column / 3); // might be redundent as this is default
                    Tiles[row / 3, column / 3] = ins;
                }
            }
        }
    }

    public void HandleSetTileAlliance(int alliance, int x, int y) {
        if (x == -1 || y == -1) {
            return;
        }

        if (Tiles[x, y]) {
            var cube = Tiles[x, y].GetComponent<Cube>();
            if (cube.Alliance != alliance) {
                if (cube.Alliance != -1) {
                    Score[cube.Alliance]--;
                    Score[alliance]++;
                }
                else {

                    Score[alliance]++;
                }
                cube.SetAlliance(alliance);
            }
        }
    }

    public string GetTilesAsString() {
        string res = "";

        for (int i = 0; i < MAP_WIDTH_REAL; i++) {
            for (int j = 0; j < MAP_HEIGHT_REAL; j++) {
                if (Tiles[i, j]) {
                    var cube = Tiles[i, j].GetComponent<Cube>();
                    if (cube) {
                        res += cube.Alliance + ",";
                    }
                }
            }
            res = res.Substring(0, res.Length - 1) + "+";
        }

        return res.Substring(0, res.Length - 1);
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
                //client.Send("CPLAYERWON|" + i);
                return true;
            }
        }

        return true;
    }

    // setup our OnEvent as callback:
    void OnEnable() {
        Debug.Log("Board OnEnable triggered");
        PhotonNetwork.OnEventCall += this.OnEvent;
    }

    // handle custom events:
    void OnEvent(byte eventcode, object content, int senderid) {
        // EndTurn

        Debug.Log("OnEvent Triggered " + eventcode + " , " + content);
        if (eventcode == 0 && !isYourTurn) {
            Debug.Log("OnEvent Triggered " + eventcode + " , " + content);
            HandleSyncTiles((string)content);
            StartTurn();
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

    public string SyncTiles() {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < MAP_WIDTH_REAL; i++) {
            for (int j = 0; j < MAP_HEIGHT_REAL; j++) {
                try {
                    var cube = Tiles[i, j].GetComponent<Cube>();
                    if (cube) {
                        if (cube.Alliance > -1) {
                            sb.Append(cube.ToString() + '+');
                        }
                    }
                }
                catch (Exception e) {
                    Debug.Log("SyncTiles: (" + i + "," + j + ") " + e.Message);
                }
            }
        }

        string tiles = sb.ToString();
        tiles = tiles.Remove(sb.Length - 1);

        sb = new StringBuilder();
        foreach (var pair in Disks) {
            if (pair.Value) {
                sb.Append(pair.Value.ToString() + '+');
            }
        }

        string disks = sb.ToString();
        if (disks.Length > 1) {
            disks = disks.Remove(disks.Length - 1);
        }

        return disks;

    }

    public void HandleSyncTiles(string tilesString) {
        int[,] tiles = new int[MAP_WIDTH_REAL, MAP_HEIGHT_REAL];

        var rows = tilesString.Split(new char[] { '+' });
        for (int i = 0; i < rows.Length; i++) {
            var array = Array.ConvertAll(rows[i].Split(new char[] { ',' }), s => int.Parse(s));
            for (int j = 0; j < array.Length; j++) {
                tiles[i, j] = array[j];
            }
        }

        for (int i = 0; i < MAP_WIDTH_REAL; i++) {
            for (int j = 0; j < MAP_HEIGHT_REAL; j++) {
                var cube = Tiles[i, j].GetComponent<Cube>();
                if (cube) {
                    cube.SetAlliance(tiles[i, j]);
                }
            }
        }
    }


    public void SyncDiscsLocationRecieved(string clientId, string data) {
        // x,z=id+
        var dots = data.Split('+');

        Score[0] = 0;
        Score[1] = 0;

        for (int i = 0; i < dots.Length; i++) {
            var stam = dots[i].Split(',');
            float x = float.Parse(stam[0]);
            float z = float.Parse(stam[1].Split('=')[0]);
            int id = int.Parse(stam[1].Split('=')[1]);

            // Set position
            var disk = Disks[id];
            if (disk) {
                disk.transform.position = new Vector3(x, transform.position.y, z);
            }
        }
    }

}
