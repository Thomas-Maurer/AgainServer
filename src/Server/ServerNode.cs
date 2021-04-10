using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ServerNode : Node
{

    [Export(PropertyHint.File, "*.cfg")] string CONFIG_FILE_NAME;
    private int SERVER_PORT;
    private int MAX_PLAYERS;

    private List<Player_Info> playersInfo = new List<Player_Info>();
    private NetworkedMultiplayerENet network;

    private bool serverIsRunning = false;

    private Color whiteColor = new Color(1,1,1,1);
    private Color greenColor = new Color(0.03f,0.73f,0.16f,1);
    private Color redColor = new Color(0.92f,0.07f,0.07f,1);
    private List<string> playersList = new List<string>();

    public override void _Ready(){
        ToggleServerButton();
        InitServerConfig();
    }

    private void StopServer() {
        network.CloseConnection();
        serverIsRunning = false;
        DisplayTextOnServerTerminal("Server Stopped", redColor);
        DisplayTextOnPlayerInfosTerminal("");
        playersList = new List<string>();
    }

    private void InitServer() {
        network = new NetworkedMultiplayerENet();
        network.CreateServer(SERVER_PORT, MAX_PLAYERS);
        GetTree().NetworkPeer = network;
        if (!(GetTree().IsConnected("network_peer_connected", this, "_player_connected"))) {
            GetTree().Connect("network_peer_connected", this, "_player_connected");
        }
        if (!(GetTree().IsConnected("network_peer_disconnected", this, "_player_disconnected"))) {
            GetTree().Connect("network_peer_disconnected", this, "_player_disconnected");
        }
        serverIsRunning = true;
        DisplayTextOnServerTerminal("Server Running", greenColor);
    }

    public void _player_connected(int id) {
        int playerId = GetTree().GetRpcSenderId();
        DisplayTextOnServerTerminal("Player Connected to server " + id.ToString());
        AddNewPlayerToList(id.ToString());
    }

    public void _player_disconnected(int id) {
        DisplayTextOnServerTerminal("Player Disconnected to server " + id.ToString());
        RemovePlayerFromList(id.ToString());
        DisplayTextOnPlayerInfosTerminal("");
    }


    [Remote]
    public void retrieveInfosBeforeLaunch(int requester) {
        DisplayTextOnServerTerminal("Get info from server before Launching the game.");
        var playerId = GetTree().GetRpcSenderId();
        this.sendCurrentPlayers(requester);
        RpcId(playerId, "allDataRetrievedFromServer", requester);
    }

    [Remote]
    public void sendCurrentPlayers (int requester) {
        int playerId = GetTree().GetRpcSenderId();
        string serverPlayers = JsonConvert.SerializeObject(playersInfo);
        RpcId(playerId, "getCurrentPlayers", serverPlayers, requester);
        DisplayTextOnServerTerminal("Sending all players : " + serverPlayers);
    }

    [Remote]
    public void addPlayerToServerList(int requester, string currentPlayer) {
        Player_Info ObjectPlayer = JsonConvert.DeserializeObject<Player_Info>(currentPlayer);
        foreach (Player_Info pInfo in playersInfo)
        {
            //Add the new Player to the Game of Other players already in Game.
            RpcId(pInfo.getNodeId(), "spawnPlayer", requester, currentPlayer);
            //Add already In game players to the New player.
            RpcId(ObjectPlayer.getNodeId(), "spawnPlayer", requester, JsonConvert.SerializeObject(pInfo));
        }
        playersInfo.Add(ObjectPlayer);
        DisplayTextOnServerTerminal(JsonConvert.SerializeObject(playersInfo, Formatting.Indented));
        RpcId(ObjectPlayer.getNodeId(), "getCurrentPlayers", JsonConvert.SerializeObject(playersInfo, Formatting.Indented), requester);
    }

    [Remote]
    public void SHello() {
        var playerId = GetTree().GetRpcSenderId();
        RpcId(playerId, "CHello");
    }

        // When start server button is pressed, launch server
    private void _on_startServerButton_pressed() {
        InitServer();
        ToggleServerButton();
    }

    private void _on_PlayerList_item_selected(int index) {
        DisplayTextOnPlayerInfosTerminal( JsonConvert.SerializeObject(playersInfo[index], Formatting.Indented));
    }

    private void _on_stopServerButton_pressed() {
        StopServer();
        ToggleServerButton();
    }

    // Init server config
    private void InitServerConfig() {
        ConfigFile configFile = new ConfigFile();
        Error err = configFile.Load(CONFIG_FILE_NAME);
        SERVER_PORT = (int) configFile.GetValue("server", "PORT", 6969);
        MAX_PLAYERS = (int) configFile.GetValue("server", "MAX_PLAYERS", 5);
    }

    private void ToggleServerButton() {
        RetrieveStartServerButtonNode().Visible = !serverIsRunning;
        RetrieveStopServerButtonNode().Visible = serverIsRunning;
    }

    private void DisplayTextOnServerTerminal(string text, Color color = new Color()) {
        DisplayTextOnTerminal("- " + text,  RetrieveServerTerminal(), color);
    }

    private void DisplayTextOnPlayerInfosTerminal(string text, Color color = new Color()) {
        DisplayTextOnTerminal(text, RetrievePlayerInfosTerminal(), color, true);
    }

    private void DisplayTextOnTerminal(string text, RichTextLabel node, Color color = new Color(), Boolean overrided = false) {
        if (color == new Color()) {
            color = whiteColor;
        }
        node.PushColor(color);
        node.Newline();
        if (overrided) {
            node.Text = text;
        } else {
            node.AddText(text);
        }
    }

    private void AddNewPlayerToList(string playerId) {
        RetrievePlayerListNode().AddItem(playerId);
        playersList.Add(playerId);
    }

    private void RemovePlayerFromList(string playerId) {
        RetrievePlayerListNode().RemoveItem(playersList.IndexOf(playerId));
        playersList.RemoveAt(playersList.IndexOf(playerId));
    }

    private ItemList RetrievePlayerListNode() {
        return GetNode<ItemList>("Interface/PlayerList");
    }

    private Button RetrieveStartServerButtonNode() {
        return GetNode<Button>("Interface/StartServerButton");
    }

    private Button RetrieveStopServerButtonNode() {
        return GetNode<Button>("Interface/StopServerButton");
    }

    private RichTextLabel RetrieveServerTerminal() {
        return GetNode<RichTextLabel>("Interface/ServerTerminal");
    }

    private RichTextLabel RetrievePlayerInfosTerminal() {
        return GetNode<RichTextLabel>("Interface/PlayerInfosTerminal");
    }
}
