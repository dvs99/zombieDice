using shared;
using System.Collections.Generic;
using UnityEngine;

/**
 * Starting state where you can connect to the server.
 */
public class LoginState : ApplicationStateWithView<LoginView>
{
    [SerializeField] private string _serverIP = null;
    [SerializeField] private int _serverPort = 0;
    [Tooltip("To avoid long iteration times, set this to true while testing.")]
    [SerializeField] private bool autoConnectWithRandomName = false;
    private bool networkError;
    public override void EnterState()
    {
        base.EnterState();
        fsm.StopBeat();

        if (networkError)
            view.TextConnectResults = "Oops, connection to server has been lost. Login again to reconnect.";

        //listen to a connect click from our view
        view.ButtonConnect.onClick.AddListener(Connect);

        //If flagged, generate a random name and connect automatically
        if (autoConnectWithRandomName)
        {
            List<string> names = new List<string> { "Pergu", "Korgulg", "Xaguk", "Rodagog", "Kodagog", "Dular", "Buggug", "Gruumsh" };
            view.userName = names[Random.Range(0, names.Count)];
            Connect();
        }
    }

    public override void ExitState ()
    {
        base.ExitState();

        //start sending heartbeats
        fsm.StartBeat();

        //stop listening to button clicks
        view.ButtonConnect.onClick.RemoveAllListeners();

        //if we get to this state again a network error has happened
        networkError = true;
    }

    /**
     * Connect to the server (with some client side validation)
     */
    private void Connect()
    {
        if (view.userName == "")
        {
            view.TextConnectResults = "Please enter a name first";
            return;
        }

        string serverIP = "";

        if (view.IP != "")
            serverIP = view.IP;
        else
            serverIP = _serverIP;

        //connect to the server and on success try to join the lobby
        if (fsm.channel.Connect(serverIP, _serverPort))
        {
            tryToJoinLobby();
        } else
        {
            view.TextConnectResults = "Oops, couldn't connect:"+string.Join("\n", fsm.channel.GetErrors());
        }
    }

    private void tryToJoinLobby()
    {
        //Construct a player join request based on the user name 
        PlayerJoinRequest playerJoinRequest = new PlayerJoinRequest();
        playerJoinRequest.name = view.userName;
        fsm.channel.SendMessage(playerJoinRequest);
    }

    /// //////////////////////////////////////////////////////////////////
    ///                     NETWORK MESSAGE PROCESSING
    /// //////////////////////////////////////////////////////////////////

    private void Update()
    {
        //if we are connected, start processing messages
        if (fsm.channel.Connected) receiveAndProcessNetworkMessages();
    }

    
    protected override void handleNetworkMessage(ASerializable pMessage)
    {
        if (pMessage is PlayerJoinResponse) handlePlayerJoinResponse (pMessage as PlayerJoinResponse);
        else if (pMessage is RoomJoinedEvent) handleRoomJoinedEvent (pMessage as RoomJoinedEvent);
    }
    

    private void handlePlayerJoinResponse(PlayerJoinResponse pMessage)
    {
        if (pMessage.result != PlayerJoinResponse.RequestResult.ACCEPTED)
        {
            if (pMessage.result == PlayerJoinResponse.RequestResult.USEDNAME)
                view.TextConnectResults = "Username in use, try a different one";

            else
                view.TextConnectResults = "Login rejected";
        }
    }

    private void handleRoomJoinedEvent (RoomJoinedEvent pMessage)
    {
        if (pMessage.room == RoomJoinedEvent.Room.LOBBY_ROOM)
        {
            fsm.ChangeState<LobbyState>();
        } 
    }

}

