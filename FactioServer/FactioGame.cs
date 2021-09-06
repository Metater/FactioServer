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

        public bool gameStarted = false;
        public bool roundResponse = false;
        public bool roundVoting = false;
        public bool roundResults = false;
        // option to make game public later
        // chat later



        private long gameStartTick;
        private long roundStartTick;
        private short playerAIndex = -1;
        private short playerBIndex = -1;
        private string playerAResponse = "Did not respond";
        private string playerBResponse = "Did not respond";

        private List<(FactioPlayer, bool)> votes = new List<(FactioPlayer, bool)>();

        public FactioGame(FactioServer factioServer, FactioPlayer leader)
        {
            this.factioServer = factioServer;
            players.Add(leader);
        }

        public void Tick(long id)
        {
            if (!gameStarted) return;
            int roundDepthSeconds = (int)((factioServer.lastTick - roundStartTick) * Program.TPS);
            switch (roundDepthSeconds)
            {
                case < 60: // Response 60
                    if (!roundResponse)
                    {
                        roundResponse = true;
                    }
                    Console.WriteLine("[Debug] Response");
                    break;
                case < 90: // Voting 90
                    if (!roundVoting)
                    {
                        roundVoting = true;
                        SendVotingStart();
                    }
                    Console.WriteLine("[Debug] Voting");
                    break;
                case < 105: // Results 105
                    if (!roundResults)
                    {
                        roundResults = true;
                        SendResultsStart();
                    }
                    Console.WriteLine("[Debug] Results");
                    break;
                default:
                    // End round
                    // Start new round
                    StartRound();
                    break;
            }
        }

        public bool TryJoinGame(FactioPlayer player)
        {
            if (gameStarted) return false;
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
            if (gameStarted) return;
            if (players.Count < 2) return;
            bool gameCanStart = true;
            foreach (FactioPlayer player in players)
            {
                if (!player.ready) gameCanStart = false;
            }
            if (gameCanStart) StartGame();
        }

        public void StartGame()
        {
            gameStartTick = factioServer.lastTick;
            gameStarted = true;
            Console.WriteLine($"[Action (Game Started)] Game started, led by \"{players[0].username}\"");
            StartRound();
        }

        public void StartRound()
        {
            roundResponse = false;
            roundVoting = false;
            roundResults = false;
            playerAResponse = "Did not respond";
            playerBResponse = "Did not respond";
            votes.Clear();

            roundStartTick = factioServer.lastTick;
            playerAIndex = GetRandomPlayerIndex();
            FactioPlayer playerA;
            FactioPlayer playerB;
            playerA = players[playerAIndex];
            playerBIndex = GetRandomPlayerIndex();
            while (playerAIndex == playerBIndex)
            {
                playerBIndex = GetRandomPlayerIndex();
            }
            playerB = players[playerBIndex];
            Scenario scenario = factioServer.scenarioRegistry.GetRandomScenario();
            SendRoundStart(playerA, new RoundStartCPacket { Scenario = scenario.text, PlayerAScenarioIndex = scenario.playerAIndex, PlayerBScenarioIndex = scenario.playerBIndex, PlayerAIndex = playerAIndex, PlayerBIndex = playerBIndex });
            SendRoundStart(playerB, new RoundStartCPacket { Scenario = scenario.text, PlayerAScenarioIndex = scenario.playerAIndex, PlayerBScenarioIndex = scenario.playerBIndex, PlayerAIndex = playerAIndex, PlayerBIndex = playerBIndex });
            foreach (FactioPlayer player in players)
            {
                if (player == playerA || player == playerB)
                    continue;
                SendRoundStart(player, new RoundStartCPacket { Scenario = scenario.text, PlayerAScenarioIndex = scenario.playerAIndex, PlayerBScenarioIndex = scenario.playerBIndex, PlayerAIndex = -1, PlayerBIndex = -1 });
            }
        }

        public void GiveResponse(FactioPlayer player, string response)
        {
            if (!gameStarted || !roundResponse) return;
            short playerIndex = (short)players.IndexOf(player);
            if (playerIndex == playerAIndex)
                playerAResponse = response;
            else if (playerIndex == playerBIndex)
                playerBResponse = response;
        }

        public void GiveVote(FactioPlayer player, bool voteIsB)
        {
            if (!gameStarted || !roundVoting) return;
            foreach ((FactioPlayer, bool) vote in votes)
                if (vote.Item1 == player) return;
            votes.Add((player, voteIsB));
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
}
