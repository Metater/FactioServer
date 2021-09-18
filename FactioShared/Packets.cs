using System;

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
    public class LeaveLobbySPacket
    {

    }
    public class ReadySPacket
    {
        // tell who is ready later
        public bool Value { get; set; }
    }
    public class ResponseSPacket
    {
        public string Response { get; set; }
    }
    public class VoteSPacket
    {
        // the chosen option
        public bool VoteIsB { get; set; }
    }
    public class ServerCommandSPacket
    {
        public string command;
        public int password;
    }


    // Could add a chat eventually
    // Client bound
    public class JoinedLobbyCPacket
    {
        public int JoinCode { get; set; }
    }
    public class CloseLobbyCPacket
    {
        public byte Reason { get; set; }
        // Server shutdown
        // Leader left
    }
    public class PlayerUpdateCPacket
    {
        public int[] PlayerIds { get; set; }
        public string[] Usernames { get; set; }
    }
    public class ReadyUpdateCPacket
    {
        public int[] ReadyPlayerIds { get; set; }
    }
    // Packets that happen within rounds
    // Server randomly decides 1 scenario per round, can make voting later
    public class RoundStartCPacket
    {
        // contains randomly gened scenario
        // contains roles
        //public int ScenarioThemeIndex { get; set; }
        public float ResponseTime { get; set; }
        public string Scenario { get; set; }
        public short PlayerAIndex { get; set; } // -1 if your not participating
        public short PlayerBIndex { get; set; } // -1 if your not participating
    }
    // Packets that happen within voting
    // Multiple groups of scenarios if more people later
    public class VotingStartCPacket
    {
        public float VotingTime { get; set; }
        // Contains responses to scenario
        public string PlayerAResponse { get; set; }
        public string PlayerBResponse { get; set; }
    }
    // Display who has already voted
    // Send when everyone has voted
    public class ResultsStartCPacket
    {
        public float ResultsTime { get; set; }
        public short PlayerAVoteCount { get; set; }
        public short PlayerBVoteCount { get; set; }
        public short PlayerAIndex { get; set; }
        public short PlayerBIndex { get; set; }
        // eventually ad everyone's score
    }
    public class RoundResultsStartCPacket
    {
        public float RoundResultsTime { get; set; }
        public string RoundResults { get; set; }
    }
    public class ServerMessageCPacket
    {
        public string Message { get; set; }
    }
}
