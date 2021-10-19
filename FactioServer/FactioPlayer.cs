using System;
using System.Collections.Generic;
using System.Text;

namespace FactioServer;

public class FactioPlayer
{
    public int clientId;
    public string username = "";

    public bool IsInGame => Game != null;

    public FactioGame Game { get; private set; }
    public bool IsReady { get; private set; } = false;
    public bool HasResponded { get; private set; } = false;
    public bool HasVoted { get; private set; } = false;

    public PlayerType playerType = PlayerType.None;

    public FactioPlayer(int clientId)
    {
        this.clientId = clientId;
    }

    public void JoinLobby(FactioGame game)
    {
        if (IsInGame)
            Game.TryLeaveLobby(this);
        Game = game;
    }
    public void LeaveLobby()
    {
        if (IsInGame)
            Game.TryLeaveLobby(this);
        Game = null;
    }

    public void Ready(bool value)
    {
        IsReady = value;
        if (IsInGame)
            Game.GiveReadyUpdate();
    }
    public void Respond(string response)
    {
        HasResponded = true;
        if (IsInGame)
            Game.GiveResponse(this, response);
    }
    public void Vote(bool voteIsB)
    {
        HasVoted = true;
        if (IsInGame)
            Game.GiveVote(this, voteIsB);
    }

    public void NewRound()
    {
        HasResponded = false;
        HasVoted = false;
        playerType = PlayerType.None;
    }

    public override string ToString()
    {
        return $"\"{username}\", id {clientId}";
    }
}

public enum PlayerType
{
    None,
    PlayerA,
    PlayerB
}