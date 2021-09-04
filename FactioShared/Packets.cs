﻿using System;

namespace FactioShared
{
    // Server bound
    public class CreateLobbySPacket
    {
        public string Username { get; set; }
    }
    public class JoinLobbySPacket
    {
        public string Username { get; set; }
        public int JoinCode { get; set; }
    }
    public class ResponseSPacket
    {
        public string Response { get; set; }
    }
    public class VoteSPacket
    {
        // the chosen option
        public short PlayerIndex { get; set; }
    }


    // Could add a chat eventually
    // Client bound
    public class JoinedLobbyCPacket
    {
        public int JoinCode { get; set; }
    }
    public class PlayerUpdateCPacket
    {
        public string[] Usernames { get; set; }
    }
    // Packets that happen within rounds
    // Server randomly decides 1 scenario per round, can make voting later
    public class RoundStartCPacket
    {
        // contains randomly gened scenario
        // contains roles
        //public int ScenarioUIIndex { get; set; }
        public string Scenario { get; set; }
        public short PlayerAScenarioIndex { get; set; }
        public short PlayerBScenarioIndex { get; set; }
        public short PlayerAIndex { get; set; } // -1 if your not participating
        public short PlayerBIndex { get; set; } // -1 if your not participating
    }
    // Packets that happen within voting
    // Multiple groups of scenarios if more people later
    public class VotingStartCPacket
    {
        // Contains responses to scenario
        public string PlayerAResponse { get; set; }
        public string PlayerBResponse { get; set; }
    }
    // Display who has already voted
    // Send when everyone has voted
    public class ResultsStartCPacket
    {
        public short WinningPlayerIndex { get; set; }
    }
}
