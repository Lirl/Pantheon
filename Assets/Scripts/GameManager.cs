using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : Photon.PunBehaviour {

    #region Public Variables

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>   
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    public byte MaxPlayersPerRoom = 2;

    /// <summary>
    /// The PUN loglevel. 
    /// </summary>
    public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;


    public static GameManager Instance { set; get; }

    public bool isHost;

    //public User user;
    public GameObject mainMenu;
    public GameObject serverMenu;
    public GameObject connectMenu;

    public GameObject serverPrefab;
    public GameObject clientPrefab;

    public InputField nameInput;

    #endregion

    #region Private Methods

    #region Private Variables


    /// <summary>
    /// This client's version number. Users are separated from each other by gameversion (which allows you to make breaking changes).
    /// </summary>
    string _gameVersion = "1";
    bool _initialized = false;
    private User user;

    #endregion


    #region MonoBehaviour CallBacks


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake() {
        Init();
    }

    void Start() {
        Init();
    }

    public void Init() {
        if (_initialized) {
            return;
        }
        _initialized = true;
        user = GameObject.FindObjectOfType<User>();

        // GameManager object setup
        Instance = this;
        DontDestroyOnLoad(gameObject);
        //user = FindObjectOfType<User>();


        // UI Setup
        //Debug.Log("GameManager Init");

        mainMenu = GameObject.Find("MainMenu");
        serverMenu = GameObject.Find("Host");
        connectMenu = GameObject.Find("Connect");

        if(serverMenu) {
            serverMenu.SetActive(false);
        }
        if(connectMenu) {
            connectMenu.SetActive(false);
        }

        //Debug.Log("serverMenu " + serverMenu);
    }

    #endregion


    #region Public Methods


    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect() {

        Debug.Log("Connect");
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.connected) {
            Debug.Log("PhotonNetwork.connected = true");
            PhotonNetwork.JoinRandomRoom();
        } else {
            Debug.Log("PhotonNetwork.ConnectUsingSettings(_gameVersion)");
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings(_gameVersion);
            PhotonNetwork.JoinRandomRoom();
        }
    }

    /// <summary>
    /// Function to be called when the user wants to quit game
    /// </summary>
    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
    }


    #endregion


    #region Photon.PunBehaviour CallBacks


    /// <summary>
    /// Called when the local player left the room. We need to load the menu scene.
    /// </summary>
    public void OnLeftRoom() {
        SceneManager.LoadScene(0);
    }


    public override void OnConnectedToMaster() {
        Debug.Log("GameManager: OnConnectedToMaster() was called by PUN");
    }


    public override void OnDisconnectedFromPhoton() {
        Debug.LogWarning("GameManagerLauncher: OnDisconnectedFromPhoton() was called by PUN");
    }

    #endregion

    void LoadGameScene() {
        if (!PhotonNetwork.isMasterClient) {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.Log("PhotonNetwork : Loading Level : " + PhotonNetwork.room.PlayerCount);
        PhotonNetwork.LoadLevel("Main");
    }

    #region Photon Messages


    public override void OnPhotonPlayerConnected(PhotonPlayer other) {
        Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.isMasterClient && PhotonNetwork.playerList.Length == 2) {
            Debug.Log("Master client loading scene for " + PhotonNetwork.playerList.Length + " players"); // called before OnPhotonPlayerDisconnected
            LoadGameScene();
        }
    }


    public override void OnPhotonPlayerDisconnected(PhotonPlayer other) {
        Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects


        if (PhotonNetwork.isMasterClient) {
            Debug.Log("OnPhotonPlayerDisonnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected
            // TODO: not sure that's suppose to be here
            LoadGameScene();
        }
    }

    #endregion



    #endregion

    public void ConnectButton() {
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }
    public void HostButton() {
        Debug.Log("HostButton");
        mainMenu.SetActive(false);
        serverMenu.SetActive(true);
    }
    public void ConnectToServerButton() {
        string hostAddress = GameObject.Find("HostInput").GetComponent<InputField>().text;

    }
    public void BackButton() {
        mainMenu.SetActive(true);
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);

        Server s = FindObjectOfType<Server>();
        if (s != null)
            Destroy(s.gameObject);

        Client c = FindObjectOfType<Client>();
        if (c != null)
            Destroy(c.gameObject);
    }
    public void HotseatButton() {
        serverMenu.SetActive(true);
        Connect();
    }

    public void StartGame() {
        Debug.Log("about to load main");
        SceneManager.LoadScene("Main");
    }

    public void OnLevelWasLoaded() {
        //Debug.Log("OnLevelWasLoaded ");        
        mainMenu = GameObject.Find("MainMenu");
        serverMenu = GameObject.Find("Host");
        connectMenu = GameObject.Find("Connect");

        serverMenu.SetActive(false);
        connectMenu.SetActive(false);

        var hotseat = GameObject.Find("HotSeat");
        hotseat.GetComponent<Button>().onClick.AddListener(HotseatButton);
        var cancelButton = serverMenu.GetComponentInChildren<Button>();
        cancelButton.GetComponent<Button>().onClick.AddListener(LeaveRoom);
        var Train = GameObject.Find("Train");
        Train.GetComponent<Button>().onClick.AddListener(StartGame);
    }

    public void SaveProgress() {
        user.Save();
    }
}
