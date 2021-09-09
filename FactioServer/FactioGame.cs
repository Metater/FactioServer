using FactioShared;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactioServer
{
    public class FactioGame : ITickable
    {
        private FactioServer factioServer;

        public List<FactioPlayer> players = new List<FactioPlayer>();
        public bool HasGameStarted { get; private set; } = false;



        private GamePhase gamePhase = GamePhase.NotStarted;

        private long gameStartTick;
        private float gameDepthSeconds => (float)((factioServer.lastTick - gameStartTick) / Program.TPS);
        private long roundStartTick;
        private float roundDepthSeconds => (float)((factioServer.lastTick - roundStartTick) / Program.TPS);
        private long phaseStartTick;
        private float phaseDepthSeconds => (float)((factioServer.lastTick - phaseStartTick) / Program.TPS);

        private short playerAIndex;
        private short playerBIndex;
        private string playerAResponse;
        private string playerBResponse;

        private bool isPhaseInit = false;

        private List<(FactioPlayer, bool)> votes = new List<(FactioPlayer, bool)>();

        public FactioGame(FactioServer factioServer, FactioPlayer leader)
        {
            this.factioServer = factioServer;
            players.Add(leader);
        }

        public void Tick(long id)
        {
            switch (gamePhase)
            {
                case GamePhase.NotStarted:
                    break;
                case GamePhase.Response:
                    if (!isPhaseInit)
                    {
                        isPhaseInit = true;
                        if (factioServer.IsDebugging) Console.WriteLine($"[Debug] Response Start, led by \"{players[0].username}\"");
                    }
                    if (phaseDepthSeconds > factioServer.configRegistry.GetFloatConfig("responseTime"))
                    {
                        UpdatePhase(GamePhase.Voting);
                    }
                    break;
                case GamePhase.Voting:
                    if (!isPhaseInit)
                    {
                        isPhaseInit = true;
                        SendVotingStart();
                        if (factioServer.IsDebugging) Console.WriteLine($"[Debug] Voting Start, led by \"{players[0].username}\"");
                    }
                    if (phaseDepthSeconds > factioServer.configRegistry.GetFloatConfig("votingTime"))
                    {
                        UpdatePhase(GamePhase.Results);
                    }
                    break;
                case GamePhase.Results:
                    if (!isPhaseInit)
                    {
                        isPhaseInit = true;
                        SendResultsStart();
                        if (factioServer.IsDebugging) Console.WriteLine($"[Debug] Results Start, led by \"{players[0].username}\"");
                    }
                    if (phaseDepthSeconds > factioServer.configRegistry.GetFloatConfig("resultsTime"))
                    {
                        StartRound();
                    }
                    break;
            }
        }

        public void LeaveGame()
        {

        }

        public bool TryJoinGame(FactioPlayer player)
        {
            if (HasGameStarted) return false;
            if (players.Contains(player)) return false;
            if (players.Exists((p) => p.username == player.username))
            {
                int number = 2;
                string username = player.username + number;
                while (players.Exists((p) => p.username == username))
                {
                    number++;
                    username = player.username + number;
                }
                player.username = username;
            }
            players.Add(player);
            return true;
        }

        public string[] GetUsernames()
        {
            List<string> usernames = new List<string>();
            foreach (FactioPlayer player in players)
            {
                usernames.Add(player.username);
            }
            return usernames.ToArray();
        }

        public void ReadyUpdate()
        {
            if (HasGameStarted) return;
            if (players.Count < 2) return;
            bool canStart = true;
            foreach (FactioPlayer player in players)
            {
                if (!player.IsReady) canStart = false;
            }
            if (canStart) StartGame();
        }

        public void StartGame()
        {
            gameStartTick = factioServer.lastTick;
            HasGameStarted = true;
            Console.WriteLine($"[Action (Game Started)] Game started, led by \"{players[0].username}\"");
            StartRound();
        }

        public void StartRound()
        {
            // Reset round state
            UpdatePhase(GamePhase.NotStarted);
            playerAResponse = "Did not respond.";
            playerBResponse = "Did not respond.";
            votes.Clear();
            players.ForEach((p) => p.NewRound());

            // Start next round
            roundStartTick = factioServer.lastTick;
            Scenario scenario = factioServer.scenarioRegistry.GetRandomScenario();
            playerAIndex = GetRandomPlayerIndex();
            playerBIndex = GetRandomPlayerIndex();
            while (playerAIndex == playerBIndex)
                playerBIndex = GetRandomPlayerIndex();
            FactioPlayer playerA = players[playerAIndex];
            FactioPlayer playerB = players[playerBIndex];
            float responseTime = factioServer.configRegistry.GetFloatConfig("responseTime");
            RoundStartCPacket participantsRoundStart = new RoundStartCPacket
            {
                ResponseTime = responseTime,
                Scenario = scenario.text,
                PlayerAScenarioIndex = scenario.playerAIndex,
                PlayerBScenarioIndex = scenario.playerBIndex,
                PlayerAIndex = playerAIndex,
                PlayerBIndex = playerBIndex
            };
            SendRoundStart(playerA, participantsRoundStart);
            SendRoundStart(playerB, participantsRoundStart);
            foreach (FactioPlayer player in players)
            {
                if (player == playerA || player == playerB)
                    continue;
                SendRoundStart(player, new RoundStartCPacket {
                    ResponseTime = responseTime,
                    Scenario = scenario.text,
                    PlayerAScenarioIndex = scenario.playerAIndex,
                    PlayerBScenarioIndex = scenario.playerBIndex,
                    PlayerAIndex = -1,
                    PlayerBIndex = -1
                });
            }
            UpdatePhase(GamePhase.Response);
        }

        public void GiveResponse(FactioPlayer player, string response)
        {
            if (gamePhase != GamePhase.Response) return;
            short playerIndex = (short)players.IndexOf(player);
            if (playerIndex == playerAIndex)
                playerAResponse = response;
            else if (playerIndex == playerBIndex)
                playerBResponse = response;
            else return;
            if (players[playerAIndex].HasResponded && players[playerBIndex].HasResponded) UpdatePhase(GamePhase.Voting);
        }

        public void GiveVote(FactioPlayer player, bool voteIsB)
        {
            if (gamePhase != GamePhase.Voting) return;
            votes.RemoveAll((playerVote) => playerVote.Item1 == player);
            votes.Add((player, voteIsB));
            foreach (FactioPlayer p in players)
            {
                Console.WriteLine("[Debug] Voted: " + p.HasVoted);
                if (!p.HasVoted) return;
            }
            UpdatePhase(GamePhase.Results);
        }

        private void UpdatePhase(GamePhase phase)
        {
            gamePhase = phase;
            phaseStartTick = factioServer.lastTick;
            isPhaseInit = false;
        }

        private void SendRoundStart(FactioPlayer player, RoundStartCPacket roundStart)
        {
            GetNetPeer(player).Send(factioServer.listener.packetProcessor.Write(roundStart), DeliveryMethod.ReliableOrdered);
        }

        private void SendVotingStart()
        {
            VotingStartCPacket votingStart = new VotingStartCPacket
            { PlayerAResponse = playerAResponse, PlayerBResponse = playerBResponse };
            players.ForEach((p) =>
            GetNetPeer(p).Send(factioServer.listener.packetProcessor.Write(votingStart), DeliveryMethod.ReliableOrdered));
        }

        private void SendResultsStart()
        {
            short playerAVoteCount = 0;
            short playerBVoteCount = 0;
            foreach ((FactioPlayer, bool) vote in votes)
            {
                if (!vote.Item2)
                    playerAVoteCount++;
                else
                    playerBVoteCount++;
            }
            ResultsStartCPacket resultsStart = new ResultsStartCPacket
            { PlayerAVoteCount = playerAVoteCount, PlayerBVoteCount = playerBVoteCount, PlayerAIndex = playerAIndex, PlayerBIndex = playerBIndex };
            players.ForEach((p) =>
            GetNetPeer(p).Send(factioServer.listener.packetProcessor.Write(resultsStart), DeliveryMethod.ReliableOrdered));
        }

        private NetPeer GetNetPeer(FactioPlayer player)
        {
            return factioServer.peerClientIdMap.GetPeer(player.clientId);
        }

        private short GetRandomPlayerIndex() { return (short)factioServer.rand.Next(0, players.Count); }
    }

    public enum GamePhase
    {
        NotStarted,
        Response,
        Voting,
        Results
    }
}
