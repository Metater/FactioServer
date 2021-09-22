using FactioShared;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FactioServer
{
    public class FactioGame : ITickable
    {
        private FactioServer factioServer;

        public int JoinCode { get; private set; }
        public List<FactioPlayer> players = new List<FactioPlayer>();
        public bool HasGameStarted => gamePhase != GamePhase.NotStarted;

        private GamePhase gamePhase = GamePhase.NotStarted;
        private bool isPhaseInit = false;

        private long phaseStartTick;
        private float phaseDepthSeconds => (float)((factioServer.lastTick - phaseStartTick) / Program.TPS);

        private Dictionary<FactioPlayer, string> playerAResponses = new Dictionary<FactioPlayer, string>(); // player, response
        private Dictionary<FactioPlayer, string> playerBResponses = new Dictionary<FactioPlayer, string>(); // player, response

        private List<Scenario> scenarios = new List<Scenario>();

        private Dictionary<FactioPlayer, bool> votes = new Dictionary<FactioPlayer, bool>();
        private Dictionary<FactioPlayer, int> scores = new Dictionary<FactioPlayer, int>(); // player, score

        private int roundIndex = 0;

        private List<FactioPlayer> unchosenPlayers = new List<FactioPlayer>();
        private List<(FactioPlayer, FactioPlayer)> chosenPairs = new List<(FactioPlayer, FactioPlayer)>();

        private (FactioPlayer, FactioPlayer) currentChosenPair;

        public FactioGame(FactioServer factioServer, int joinCode, FactioPlayer leader)
        {
            this.factioServer = factioServer;
            JoinCode = joinCode;
            players.Add(leader);
        }

        public void Tick(long id)
        {
            switch (gamePhase)
            {
                case GamePhase.NotStarted:
                    TickPhaseNotStarted();
                    break;
                case GamePhase.Response:
                    TickPhaseResponse();
                    break;
                case GamePhase.Voting:
                    TickPhaseVoting();
                    break;
                case GamePhase.Results:
                    TickPhaseResults();
                    break;
                case GamePhase.RoundResults:
                    TickPhaseRoundResults();
                    break;
            }
        }

        #region TickPhaseMethods
        private void TickPhaseNotStarted()
        {
            if (ShouldPhaseInit()) 
            {

            }
        }
        private void TickPhaseResponse()
        {
            if (ShouldPhaseInit())
            {
                playerAResponses.Clear();
                playerBResponses.Clear();

                if (factioServer.IsDebugging) Program.LogLine(LoggingTag.FactioGame, $"Response Start, led by {players[0]}", true);
            }
            if (phaseDepthSeconds > factioServer.configRegistry.GetFloatConfig("responseTime"))
            {
                UpdatePhase(GamePhase.Voting);
            }
        }
        private void TickPhaseVoting()
        {
            if (ShouldPhaseInit())
            {
                votes.Clear();

                if (factioServer.IsDebugging) Program.LogLine(LoggingTag.FactioGame, $"Voting Start, led by {players[0]}", true);

                currentChosenPair = chosenPairs.Last();
                chosenPairs.Remove(currentChosenPair);
                string playerAResponse = "Did not respond.";
                string playerBResponse = "Did not respond.";
                if (playerAResponses.TryGetValue(currentChosenPair.Item1, out string outPlayerAResponse))
                    playerAResponse = outPlayerAResponse;
                else
                    playerAResponses.Add(currentChosenPair.Item1, playerAResponse);
                if (playerBResponses.TryGetValue(currentChosenPair.Item2, out string outPlayerBResponse))
                    playerBResponse = outPlayerBResponse;
                else
                    playerBResponses.Add(currentChosenPair.Item2, playerBResponse);
                VotingStartCPacket votingStart = new VotingStartCPacket
                { VotingTime = factioServer.configRegistry.GetFloatConfig("votingTime"), PlayerAResponse = playerAResponse, PlayerBResponse = playerBResponse };
                SendVotingStart(votingStart);
            }
            if (phaseDepthSeconds > factioServer.configRegistry.GetFloatConfig("votingTime"))
            {
                UpdatePhase(GamePhase.Results);
            }
        }
        private void TickPhaseResults()
        {
            if (ShouldPhaseInit())
            {
                if (factioServer.IsDebugging) Program.LogLine(LoggingTag.FactioGame, $"Results Start, led by {players[0]}", true);

                Scenario scenario = scenarios.Last();
                string results = "Scenario:\n";
                FactioPlayer playerA = currentChosenPair.Item1;
                FactioPlayer playerB = currentChosenPair.Item2;
                results += $"\t{scenario.Compile(playerA.username, playerB.username)}\n\n";
                int playerAVotes = 0;
                int playerBVotes = 0;
                foreach (KeyValuePair<FactioPlayer, bool> vote in votes)
                {
                    if (!vote.Value) playerAVotes++;
                    else playerBVotes++;
                }
                results += $"Votes\n";
                results += $"{playerA.username}: {playerAVotes}, {playerB.username}: {playerBVotes}\n\n";
                int totalVotes = playerAVotes + playerBVotes;
                if (totalVotes == 0)
                {
                    results += $"Really, nobody voted, lame!";
                }
                else
                {
                    int playerAPoints = (int)(((float)playerAVotes / (float)totalVotes) * 1000f);
                    int playerBPoints = (int)(((float)playerBVotes / (float)totalVotes) * 1000f);
                    if (playerAVotes == playerBVotes) // Tie
                    {
                        results += $"Everyone is a winner, now take your participation trophy!\n";
                        results += $"Tied!\n";
                    }
                    else if (playerAVotes > playerBVotes) // Player A
                    {
                        playerAPoints += 500;
                        results += $"Winner: {playerA.username}\n";
                        results += $"Winning Response: {playerAResponses[playerA]}\n\n";
                    }
                    else // Player B
                    {
                        playerBPoints += 500;
                        results += $"Winner: {playerB.username}\n";
                        results += $"Winning Response: {playerBResponses[playerB]}\n\n";
                    }
                    results += $"Points\n";
                    results += $"{playerA.username}: +{playerAPoints}, {playerB.username}: +{playerBPoints}";
                    if (scores.ContainsKey(playerA))
                        scores[playerA] += playerAPoints;
                    else
                        scores.Add(playerA, playerAPoints);
                    if (scores.ContainsKey(playerB))
                        scores[playerB] += playerBPoints;
                    else
                        scores.Add(playerB, playerBPoints);
                }
                ResultsStartCPacket resultsStart = new ResultsStartCPacket
                { ResultsTime = factioServer.configRegistry.GetFloatConfig("resultsTime"), Results = results };
                SendResultsStart(resultsStart);
            }
            if (phaseDepthSeconds > factioServer.configRegistry.GetFloatConfig("resultsTime"))
            {
                if (chosenPairs.Count > 0)
                {
                    votes.Clear();

                    UpdatePhase(GamePhase.Voting);
                }
                else
                    UpdatePhase(GamePhase.RoundResults);
            }
        }
        private void TickPhaseRoundResults()
        {
            if (ShouldPhaseInit())
            {
                if (factioServer.IsDebugging) Program.LogLine(LoggingTag.FactioGame, $"Round Results Start, led by {players[0]}", true);

                List<KeyValuePair<FactioPlayer, int>> unsortedScores = new List<KeyValuePair<FactioPlayer, int>>(scores);
                List<KeyValuePair<FactioPlayer, int>> sortedScores = new List<KeyValuePair<FactioPlayer, int>>();
                for (int i = 0; i < scores.Count || i < 10; i++)
                {
                    KeyValuePair<FactioPlayer, int> greatestScore = new KeyValuePair<FactioPlayer, int>(null, int.MinValue);
                    for (int j = 0; j < unsortedScores.Count; j++)
                    {
                        if (unsortedScores[j].Value > greatestScore.Value)
                        {
                            greatestScore = unsortedScores[j];
                            unsortedScores.RemoveAt(j);
                            sortedScores.Add(greatestScore);
                            break;
                        }
                    }
                }

                string roundResults = "Leaderboard:\n\n";
                foreach (KeyValuePair<FactioPlayer, int> score in sortedScores)
                    roundResults += $"{score.Key.username}: {score.Value}\n";
                RoundResultsStartCPacket roundResultsStart = new RoundResultsStartCPacket
                { RoundResultsTime = factioServer.configRegistry.GetFloatConfig("roundResultsTime"), RoundResults = roundResults };
                SendRoundResultsStart(roundResultsStart);
            }
            if (phaseDepthSeconds > factioServer.configRegistry.GetFloatConfig("roundResultsTime"))
            {
                EndRound();
            }
        }
        #endregion TickPhaseMethods

        #region NetworkMethods
        private void SendReadyUpdate(ReadyUpdateCPacket readyUpdate)
        {
            foreach (FactioPlayer player in players)
                GetNetPeer(player).Send(factioServer.listener.packetProcessor.Write(readyUpdate), DeliveryMethod.ReliableOrdered);
        }
        private void SendRoundStart(FactioPlayer player, RoundStartCPacket roundStart)
        {
            GetNetPeer(player).Send(factioServer.listener.packetProcessor.Write(roundStart), DeliveryMethod.ReliableOrdered);
        }
        private void SendVotingStart(VotingStartCPacket votingStart)
        {
            foreach (FactioPlayer player in players)
                GetNetPeer(player).Send(factioServer.listener.packetProcessor.Write(votingStart), DeliveryMethod.ReliableOrdered);
        }
        private void SendResultsStart(ResultsStartCPacket resultsStart)
        {
            foreach (FactioPlayer player in players)
                GetNetPeer(player).Send(factioServer.listener.packetProcessor.Write(resultsStart), DeliveryMethod.ReliableOrdered);
        }
        private void SendRoundResultsStart(RoundResultsStartCPacket roundResultsStart)
        {
            foreach (FactioPlayer player in players)
                GetNetPeer(player).Send(factioServer.listener.packetProcessor.Write(roundResultsStart), DeliveryMethod.ReliableOrdered);
        }
        #endregion NetworkMethods

        #region GameEntryMethods
        public bool TryJoinLobby(FactioPlayer player)
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
        public bool TryLeaveLobby(FactioPlayer player)
        {
            if (!players.Contains(player)) return false;
            int playerIndex = players.IndexOf(player);

            if (playerIndex == 0) CloseLobby(LobbyClose.LeaderLeft);
            if (players.Count - 1 <= 1) CloseLobby(LobbyClose.OnlyPlayer);

            players.Remove(player);
            return true;
        }
        public (int[], string[]) GetPlayers()
        {
            List<int> playerIds = new List<int>();
            List<string> usernames = new List<string>();
            foreach (FactioPlayer player in players)
            {
                playerIds.Add(player.clientId);
                usernames.Add(player.username);
            }
            return (playerIds.ToArray(), usernames.ToArray());
        }
        #endregion GameEntryMethods

        #region GameLogicUpdateMethods
        private void StartGame()
        {
            roundIndex = 0;
            scores.Clear();

            Program.LogLine(LoggingTag.FactioGame, $"Game started, led by {players[0]}");

            StartRound();
        }
        private void StartRound()
        {
            Program.LogLine(LoggingTag.FactioGame, $"Round {roundIndex} started, led by {players[0]}");

            UpdatePhase(GamePhase.Response);
            players.ForEach((p) => p.NewRound());
            Scenario scenario = GetNextScenario();
            unchosenPlayers = players.ToList();
            float responseTime = factioServer.configRegistry.GetFloatConfig("responseTime");
            while (unchosenPlayers.Count > 1)
            {
                FactioPlayer playerA = ChooseRandomPlayer();
                FactioPlayer playerB = ChooseRandomPlayer();
                playerA.playerType = PlayerType.PlayerA;
                playerB.playerType = PlayerType.PlayerB;
                chosenPairs.Add((playerA, playerB));
                RoundStartCPacket participantsRoundStart = new RoundStartCPacket
                {
                    ResponseTime = responseTime,
                    Scenario = scenario.text,
                    PlayerAId = playerA.clientId,
                    PlayerBId = playerB.clientId
                };
                SendRoundStart(playerA, participantsRoundStart);
                SendRoundStart(playerB, participantsRoundStart);
            }
            if (unchosenPlayers.Count == 1)
            {
                FactioPlayer unchosenPlayer = unchosenPlayers[0];
                SendRoundStart(unchosenPlayer, new RoundStartCPacket
                {
                    ResponseTime = responseTime,
                    Scenario = scenario.text,
                    PlayerAId = -1,
                    PlayerBId = -1
                });
            }
        }
        private void EndRound()
        {
            if (roundIndex < factioServer.configRegistry.GetIntConfig("roundsPerGame") - 1)
            {
                roundIndex++;
                StartRound();
            }
            else
                EndGame();
        }
        private void EndGame()
        {
            Program.LogLine(LoggingTag.FactioGame, $"Game ended, led by {players[0]}");

            UpdatePhase(GamePhase.NotStarted);
            players.ForEach((p) => factioServer.gameManager.JoinedLobby(GetNetPeer(p), JoinCode));
        }
        #endregion GameLogicUpdateMethods

        #region PlayerInputMethods
        public void GiveReadyUpdate()
        {
            if (HasGameStarted) return;
            List<int> readyPlayerIds = new List<int>();
            bool canStart = true;
            foreach (FactioPlayer player in players)
            {
                if (!player.IsReady) canStart = false;
                else readyPlayerIds.Add(player.clientId);
            }
            SendReadyUpdate(new ReadyUpdateCPacket { ReadyPlayerIds = readyPlayerIds.ToArray() });
            if (players.Count < 2) return;
            if (canStart) StartGame();
        }

        public void GiveResponse(FactioPlayer player, string response)
        {
            if (gamePhase != GamePhase.Response) return;
            switch (player.playerType)
            {
                case PlayerType.None:
                    return;
                case PlayerType.PlayerA:
                    if (playerAResponses.ContainsKey(player))
                        playerAResponses[player] = response;
                    else
                        playerAResponses.Add(player, response);
                    break;
                case PlayerType.PlayerB:
                    if (playerBResponses.ContainsKey(player))
                        playerBResponses[player] = response;
                    else
                        playerBResponses.Add(player, response);
                    break;
            }
            foreach ((FactioPlayer, FactioPlayer) chosenPair in chosenPairs)
            {
                if (!playerAResponses.ContainsKey(chosenPair.Item1)) return;
                if (!playerBResponses.ContainsKey(chosenPair.Item2)) return;
            }
            UpdatePhase(GamePhase.Voting);
        }

        public void GiveVote(FactioPlayer player, bool voteIsB)
        {
            if (gamePhase != GamePhase.Voting) return;
            if (votes.ContainsKey(player))
                votes[player] = voteIsB;
            else
                votes.Add(player, voteIsB);
            foreach (FactioPlayer p in players) 
                if (!p.HasVoted) return;
            UpdatePhase(GamePhase.Results);
        }
        #endregion PlayerInputMethods

        #region GenericMethods
        public void CloseLobby(LobbyClose reason)
        {
            CloseLobbyCPacket closeLobby = new CloseLobbyCPacket
            { Reason = (byte)reason };
            players.ForEach((p) =>
            GetNetPeer(p).Send(factioServer.listener.packetProcessor.Write(closeLobby), DeliveryMethod.ReliableOrdered));
            Program.LogLine(LoggingTag.FactioGame, $"Lobby closed, led by {players[0]}");
            factioServer.gameManager.CloseLobby(this);
        }
        private void UpdatePhase(GamePhase phase)
        {
            gamePhase = phase;
            phaseStartTick = factioServer.lastTick;
            isPhaseInit = false;
        }
        private bool ShouldPhaseInit()
        {
            bool shouldPhaseInit = !isPhaseInit;
            isPhaseInit = true;
            return shouldPhaseInit;
        }
        private NetPeer GetNetPeer(FactioPlayer player)
        {
            return factioServer.peerClientIdMap.GetPeer(player.clientId);
        }
        private FactioPlayer ChooseRandomPlayer()
        {
            int chosenPlayerIndex = factioServer.rand.Next(0, unchosenPlayers.Count);
            FactioPlayer chosenPlayer = unchosenPlayers[chosenPlayerIndex];
            unchosenPlayers.RemoveAt(chosenPlayerIndex);
            return chosenPlayer;
        }
        private Scenario GetNextScenario()
        {
            Scenario scenario = factioServer.scenarioRegistry.GetRandomScenario();
            int tries = 0;
            while (scenarios.Contains(scenario))
            {
                scenario = factioServer.scenarioRegistry.GetRandomScenario();
                if (tries > 10) scenarios.Clear();
                tries++;
            }
            scenarios.Add(scenario);
            return scenario;
        }
        #endregion GenericMethods
    }

    public enum GamePhase
    {
        NotStarted,
        Response,
        Voting,
        Results,
        RoundResults
    }
}
