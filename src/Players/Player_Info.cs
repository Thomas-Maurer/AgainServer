using Godot;
using System;

public class Player_Info
{    
    public string name;
    public int nodeId;
    public bool isServer;
    
    public Player_Info(string name, int nodeId, bool isServer = false) {
        this.name = name;
        this.nodeId = nodeId;
        this.isServer = isServer;
    }

    public string getPlayerName() {
        return this.name;
    }

    public int getNodeId() {
        return this.nodeId;
    }

    public override string ToString()
    {
        return string.Format("{0} - {1}", name, nodeId);
    }
}
