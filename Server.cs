using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;


public class Server : Node
{
    private int SERVER_PORT = 6969;
    private int MAX_PLAYERS = 5;

    private List<Player_Info> playersInfo = new List<Player_Info>();
    private NetworkedMultiplayerENet network = new NetworkedMultiplayerENet(); 


    public void initServer()
    {
        network.CreateServer(SERVER_PORT, MAX_PLAYERS);
        this.GetTree().NetworkPeer = network;
        this.GetTree().Connect("network_peer_connected", this, "_player_connected");
        this.GetTree().Connect("network_peer_disconnected", this, "_player_disconnected");
    }

    public void _player_connected(int id) {
        var playerId = GetTree().GetRpcSenderId();
        GD.Print("Player Connected to server " + id.ToString());
    }

    public void _player_disconnected(int id) {
        GD.Print("Player Disconnected to server " + id.ToString());
    }

    public override void _Ready(){
        this.initServer();
    }

    [Remote]
    public void retrieveInfosBeforeLaunch(int requester) {
        GD.Print("Get info from server before Launching the game.");
        var playerId = GetTree().GetRpcSenderId();
        this.sendCurrentPlayers(requester);
        RpcId(playerId, "allDataRetrievedFromServer", requester);
    }

    [Remote]
    public void sendCurrentPlayers (int requester) {
        var playerId = GetTree().GetRpcSenderId();
        string serverPlayers = JsonConvert.SerializeObject(playersInfo);
        RpcId(playerId, "getCurrentPlayers", serverPlayers, requester);
        GD.Print("Sending all players : " + serverPlayers);
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
        GD.Print(JsonConvert.SerializeObject(playersInfo, Formatting.Indented));
        RpcId(ObjectPlayer.getNodeId(), "getCurrentPlayers", JsonConvert.SerializeObject(playersInfo, Formatting.Indented), requester);
        GD.Print("Player added to the server : " + currentPlayer);
    }

    [Remote]
    public void SHello() {
        var playerId = GetTree().GetRpcSenderId();
        RpcId(playerId, "CHello");
    }
}
