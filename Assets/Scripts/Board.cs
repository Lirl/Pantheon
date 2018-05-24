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

    public GameObject DummyDisk;

    public CanvasGroup alertCanvas;
    private GameObject yourScore;
    private GameObject opponentScore;
    private float lastAlert;
    private bool alertActive;


    private bool gameIsOver;
    public float gameTime = 10f;

    public bool isYourTurn;
    public bool TurnHasEnded { get; private set; }
    public List<Disk> DisksList = new List<Disk>();

    private Vector2 mouseOver;
    private Vector3 prevDiskIdleResult = new Vector3(-1, -1, -1);

    public List<CardInformation> CardTypes = new List<CardInformation>();

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

    #region AI Properties
    public bool isTutorial = false;
    public GameObject AILastCreatedDisk;
    public bool AIAimDiskPositionChosen;
    public Vector3 AIAimDiskPosition;
    #endregion

    private Disk currentlyReleasedDisk;
    private bool _diskIdleTriggered;

    private void Start() {
        Instance = this;

        // UI Setop
        Hand = GameObject.Find("Hand");
        TimeMessage = GameObject.Find("TimeMessage") as GameObject;

        alertCanvas = GameObject.Find("AlertText").GetComponent<CanvasGroup>();
        yourScore = GameObject.Find("YourScore");
        opponentScore = GameObject.Find("OpponentScore");

        WinMessage.SetActive(false);
        LoseMessage.SetActive(false);
        BackToMenu.SetActive(false);


        // Check player connectivity
        if (PhotonNetwork.connected) {
            isHost = GameManager.Instance.isHost;
        } else {
            isHost = true;
        }

        // The only way this condition will suffies
        // is when the user has entered his first game, which loads this scene without being
        // connected to a room
        if(!PhotonNetwork.inRoom) {
            isTutorial = true;
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

        if(isTutorial) {
            StartTurnTutorial();
        } else if (isHost) {
            // Host starts
            StartTurn();
            Invoke("CreatePowerUp", 1f);
            Invoke("CheckWinner", gameTime);
        }
    }

    #region Disk Management

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

    internal GameObject CreateDisk(int alliance, int code) {
        Debug.Log("CreateDisk excepted. alliance = " + alliance + " code = " + code);
        GameObject hook = GetHook(alliance);

        Debug.Log("Attempting to load prefab " + "Characters/Character" + code + " for alliance " + alliance + " isYourTurn " + isYourTurn);
        var prefab = Resources.Load("Characters/Character" + code) as GameObject;
        var offset = (code == 3 ? 7f : 3f);

        GameObject ins;
        if (PhotonNetwork.connected) {
            ins = PhotonNetwork.Instantiate("Characters/Character" + code, hook.transform.position + new Vector3(0, 3f, 0), Quaternion.identity, 0);
        } else {
            ins = Instantiate(prefab, new Vector3(hook.transform.position.x, hook.transform.position.y + 1.5f, hook.transform.position.z), Quaternion.identity);
        }
        ins.GetComponent<SpringJoint>().connectedBody = hook.GetComponent<Rigidbody>();
        ins.GetComponent<Disk>().Init(alliance, isYourTurn);

        // Handle UI
        if (Hand) {
            Hand.SetActive(false);
        }

        return ins;
    }

    public GameObject GetHook(int alliance) {
        if (alliance == 1) {
            return GameObject.Find("HostHook");
        } else {
            return GameObject.Find("ClientHook");
        }
    }

    internal GameObject CreateDisk(int alliance) {
        GameObject hook;
        if (alliance == 1) {
            hook = GameObject.Find("HostHook");
        } else {
            hook = GameObject.Find("ClientHook");
        }
        GameObject ins;
        if(PhotonNetwork.connected) {
             ins = PhotonNetwork.Instantiate(DummyDisk.name, hook.transform.position, Quaternion.identity, 0);
        } else {
            ins = Instantiate(DummyDisk, hook.transform.position, Quaternion.identity);
        }
        
        ins.GetComponent<Disk>().Init(alliance);

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

    public void OnDisksIdleTrigger() {
        var pos = new Vector3(0, 0, 0);
        try {
            for(int i = 0; i < DisksList.Count; i++) {
                if (DisksList[i]) {
                    pos += DisksList[i].gameObject.GetComponent<Rigidbody>().velocity;
                }
            }
        } catch(Exception e) {
            Debug.Log("Exception : " + e.Message);
        }

        Debug.Log(pos);

        var thresh = pos - prevDiskIdleResult;

        // Meaning disks stopped moving
        if (Math.Abs(thresh.x + thresh.z) < 1) {
            if (currentlyReleasedDisk) {
                currentlyReleasedDisk.ForceSyncPosition();
            }

            OnDisksIdle();
        } else {
            prevDiskIdleResult = pos;
            Invoke("OnDisksIdleTrigger", 1);
        }
    }

    void OnDestroy() {
        Debug.Log("I was killed? :(");
    }

    public void OnDisksIdle() {
        Debug.Log("OnDisksIdle");
        if(!_diskIdleTriggered) {
            _diskIdleTriggered = true;
            if(isTutorial) {
                EndTurnTutorial();
            } else {
                EndTurn();
            }
        }
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

    internal void SaveDisk(int id, Disk disk) {
        Disks.Add(id, disk);
        DisksList.Add(disk);
    }

    #endregion

    #region AI

    public int GetAICardCode() {

        //TODO: continue here
        switch(TurnCounter) {
            case 2:
                
                // First move by the player, summon a priest
                return 3; // groot

            default:
                break;

        }

        return UnityEngine.Random.Range(0, 3);
    }

    public void AIAimDisk() {
        Debug.Log("AIAimDisk " + (isYourTurn ? "your turn" : "not your turn"));
        // Find all disks of the opponent (human player)
        var hook = GetHook(0);
        var enemies = DisksList.FindAll(disk => disk && disk.Alliance == 1);
        enemies.Sort(delegate (Disk d1, Disk d2) {
            if (Vector3.Distance(d1.gameObject.transform.position, hook.transform.position) > Vector3.Distance(d2.transform.position, hook.transform.position)) {
                return 1;
            } else {
                return -1;
            }
        });

        /*Vector3 aim = (enemies[0].transform.position - hook.transform.position).normalized;
        AIAimDiskPosition = hook.transform.position + (aim * -40);*/

        AIAimDiskPosition = new Vector3(AILastCreatedDisk.transform.position.x + UnityEngine.Random.Range(-20.0f, 20.0f), AILastCreatedDisk.transform.position.y, AILastCreatedDisk.transform.position.z + UnityEngine.Random.Range(10.0f, 30.0f));

        // So SpringJoint will not drag it out off aiming position
        AILastCreatedDisk.GetComponent<Rigidbody>().isKinematic = true;

        // Start aiming
        AIAimDiskPositionChosen = true;
    }

    // Triggered in update when AIAimDiskPositionChosen == true
    public void HandleAIAiming() {
        if(!isTutorial || !AIAimDiskPositionChosen) {
            return;
        }

        float step = 12 * Time.deltaTime;
        if (Vector3.Distance(AILastCreatedDisk.transform.position, AIAimDiskPosition) < 0.1) {
            AIAimDiskPositionChosen = false;
            AIReleaseDisk();
        } else {
            AILastCreatedDisk.transform.position = Vector3.MoveTowards(AILastCreatedDisk.transform.position, AIAimDiskPosition, step);
        }
    }

    public void AIReleaseDisk() {
        Debug.Log("AIReleaseDisk");
        AILastCreatedDisk.GetComponent<Disk>().Release(AIAimDiskPosition);
    }

    #endregion

    #region Tutorial Functions

    public void StartTurnTutorial() {
        // Human Player
        if (isHost) {
            isYourTurn = true;
            DrawCard();
            if (Hand) {
                Hand.SetActive(true);
            }

        } else {
            // AI
            Invoke("AIMove", 1);
        }
        isZoomedOut = true;
    }

    public void AIMove() {
        int cardCode = GetAICardCode();
        AILastCreatedDisk = CreateDisk(0, cardCode);

        Invoke("AIAimDisk", 2);
    }

    public void EndTurnTutorial() {

        // Human Player
        if (isHost) {
            if (Hand) {
                Hand.SetActive(false);
            }
        }

        isHost = !isHost;
        StartTurnTutorial();
    }

    #endregion

    #region Game Loop

    public void StartTurn() {
        if (gameIsOver) {
            return;
        }

        isYourTurn = true;
        TurnHasEnded = false;

        Alert("Player " + (isHost ? 1 : 2) + " turn #" + TurnCounter + " YourTurn: " + isYourTurn);
        Debug.Log("Player " + (isHost ? 1 : 2) + " turn #" + TurnCounter + " YourTurn: " + isYourTurn + " isHost: " + isHost);

        Alert("Player " + (isHost ? 1 : 2) + " turn #" + TurnCounter);
        // Add a new card to player hand (current turn)
        DrawCard();

        // 10 seconds turn
        // TODO: add end turn indication
        //Invoke("EndTurn", 10);
        
        if (Hand) {
            Hand.SetActive(true);
        }
        
        isZoomedOut = true;
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

    private void EndTurn() {
        Debug.Log("EndTurn " + (isYourTurn ? "your turn" : "not your turn"));
        if (isYourTurn && !TurnHasEnded) {
            TurnHasEnded = true;

            // That's all you need to do for switching the turn
            // BoardSync will sync that data, and trigger SetTurn on the other player.
            // By doing so, the other player will trigger StartTurn, that will continue game loop as normal
            TurnCounter++;
            // We shouldnt sync tiles if we are not connected

            isYourTurn = false;

            if (Hand) {
                Hand.SetActive(false);
            }

            if (PhotonNetwork.connected) {
                PhotonNetwork.RaiseEvent(0, GetTilesAsString(), true, null);
            } else {
                isHost = !isHost;
                StartTurn();
            }
        }
    }

    public void CheckWinner() {
        Debug.Log("CheckWinner");
        if (Score[0] >= Score[1]) {
            HandleShowWinner(0);
        } else {
            HandleShowWinner(1);
        }

        gameIsOver = true;
    }

    internal void HandleShowWinner(int alliance) {
        Debug.Log("HandleShowWinner started");
        if (Hand) {
            Hand.SetActive(false);
        }

        if (alliance == (isHost ? 1 : 0)) {
            WinMessage.SetActive(true);
        } else {
            LoseMessage.SetActive(true);
        }
        BackToMenu.SetActive(true);
    }
    #endregion

    private void Update() {

        gameTime -= Time.deltaTime;
        if (gameTime >= 0 && !gameIsOver) {
            TimeMessage.GetComponentInChildren<Text>().text = Math.Floor(gameTime).ToString();
        }

        yourScore.GetComponentInChildren<Text>().text = Score[isHost ? 1 : 0].ToString();
        opponentScore.GetComponentInChildren<Text>().text = Score[isHost ? 0 : 1].ToString();

        if (isZoomedIn) {
            if (Camera.main.orthographicSize >= 45) {
                Camera.main.orthographicSize -= 0.5f;
            } else {
                isZoomedIn = false;
            }
        }

        if (isZoomedOut) {
            if (Camera.main.orthographicSize <= 75) {
                Camera.main.orthographicSize += 0.5f;
            } else {
                isZoomedOut = false;
            }
        }

        UpdateAlert();
        UpdateMouseOver();
        HandleAIAiming();
    }

    #region Power Ups

    internal void CreatePowerUp() {
        int x = UnityEngine.Random.Range(1, MAP_WIDTH_REAL - 1);
        int y = UnityEngine.Random.Range(1, MAP_HEIGHT_REAL - 1);
        int code = UnityEngine.Random.Range(0, powerUpsAmount);

        if (PhotonNetwork.connected) {
            photonView.RPC("HandleCreatePowerUp", PhotonTargets.All, code, x, y);
        } else {
            PunHandleCreatePowerUp(code, x, y);
        }
    }

    [PunRPC]
    internal GameObject PunHandleCreatePowerUp(int code, int x, int y) {
        var toInstantiate = Resources.Load("Characters/PowerUps" + code) as GameObject;
        if (Tiles[x, y] && toInstantiate) {
            GameObject rune;
            if (PhotonNetwork.connected) {
                rune = PhotonNetwork.Instantiate("Characters/PowerUps" + code, Tiles[x, y].transform.position + new Vector3(0, 2f, 0), Quaternion.Euler(new Vector3(45, 45, 45)), 0);
            } else {
                rune = Instantiate(toInstantiate, Tiles[x, y].transform.position + new Vector3(0, 2f, 0), Quaternion.Euler(new Vector3(45, 45, 45)));
            }
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

    #endregion

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

    private void GenerateBoard() {
        // Create deck list
        Deck = new List<int>();
        for (int i = 0; i < 30; i++) {
            if (User.instance != null) {
                Deck.Add(User.instance.deck[UnityEngine.Random.Range(0, User.instance.deck.Count)]); // UnityEngine.Random.Range(0, 4));
            } else {
                Deck.Add(UnityEngine.Random.Range(0, 3));
            }
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
        if (PhotonNetwork.connected) {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("PunHandleSetTileAlliance", PhotonTargets.All, alliance, x, y);
        } else {
            PunHandleSetTileAlliance(alliance, x, y);
        }
    }

    [PunRPC]
    public void PunHandleSetTileAlliance(int alliance, int x, int y) {
        //Debug.Log("PunHandleSetTileAlliance : " + alliance + "," + x + "," + y);
        if (x == -1 || y == -1) {
            return;
        }

        if (Tiles[x, y]) {
            var cube = Tiles[x, y].GetComponent<Cube>();
            if (cube.Alliance != alliance) {
                if (cube.Alliance != -1) {
                    Score[cube.Alliance]--;
                    Score[alliance]++;
                } else {
                    Score[alliance]++;
                }
                Debug.Log("cube.SetAlliance called");
                cube.SetAlliance(alliance);
            }
        } else {
            Debug.LogWarning("Attempting to set cube " + x + ", " + y + " but Tiles[x,y] is null");
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
                } catch (Exception e) {
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

    public void OnDiskReleased(Disk disk) {
        Debug.Log("OnDiskReleased");
        currentlyReleasedDisk = disk;

        if (disk.Alliance == (isHost ? 1 : 0) || isTutorial) {
            // This is the player that played the move
            // He should end the turn
            _diskIdleTriggered = false;
            Invoke("OnDisksIdle", 3);
            OnDisksIdleTrigger();
        }
    }

    internal void OnDiskClick(Disk disk) {
        isZoomedIn = false;
        isZoomedOut = true;
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

    public int Clamp(int value, int min, int max) {
        return (value < min) ? min : (value > max) ? max : value;
    }

}
