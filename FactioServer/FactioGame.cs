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

        private long gameStartTick;
        //private float gameDepthSeconds => (float)((factioServer.lastTick - gameStartTick) / Program.TPS);
        private long roundStartTick;
        //private float roundDepthSeconds => (float)((factioServer.lastTick - roundStartTick) / Program.TPS);
        private long phaseStartTick;
        private float phaseDepthSeconds => (float)((factioServer.lastTick - phaseStartTick) / Program.TPS);

        private Dictionary<int, string> playerAResponses = new Dictionary<int, string>(); // playerId, response
        private Dictionary<int, string> playerBResponses = new Dictionary<int, string>(); // playerId, response

        private List<Scenario> scenarios = new List<Scenario>();

        private List<(FactioPlayer, bool)> votes = new List<(FactioPlayer, bool)>();
        private Dictionary<int, int> scores = new Dictionary<int, int>(); // playerId, score

        private int roundCount = 0;

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
                if (factioServer.IsDebugging) Program.LogLine(LoggingTag.FactioGame, $"Response Start, led by \"{players[0].username}\"", true);
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

            }
        }
        private void TickPhaseResults()
        {
            if (ShouldPhaseInit())
            {

            }
        }
        private void TickPhaseRoundResults()
        {
            if (ShouldPhaseInit())
            {

            }
        }
        #endregion TickPhaseMethods

        #region NetworkMethods
        private void SendReadyUpdate()
        {

        }
        private void SendRoundStart(FactioPlayer player, RoundStartCPacket roundStart)
        {

        }
        private void SendVotingStart()
        {

        }
        private void SendResultsStart()
        {

        }
        private void SendRoundResultsStart()
        {

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
            if (players.Count <= 1) CloseLobby(LobbyClose.OnlyPlayer);

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
            gameStartTick = factioServer.lastTick;
            Program.LogLine(LoggingTag.FactioGame, $"Game started, led by \"{players[0].username}\"");
            StartRound();
        }
        private void EndGame()
        {
            Program.LogLine(LoggingTag.FactioGame, $"Game ended, led by \"{players[0].username}\"");
        }
        private void StartRound()
        {

        }
        private void UpdatePhase(GamePhase phase)
        {
            gamePhase = phase;
            phaseStartTick = factioServer.lastTick;
            isPhaseInit = false;
        }
        #endregion GameLogicUpdateMethods

        #region PlayerInputMethods
        public void GiveReadyUpdate()
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

        public void GiveResponse(FactioPlayer player, string response)
        {
            /*
            if (gamePhase != GamePhase.Response) return;
            short playerIndex = (short)players.IndexOf(player);
            if (playerIndex == playerAIndex)
                playerAResponse = response;
            else if (playerIndex == playerBIndex)
                playerBResponse = response;
            else return;
            if (players[playerAIndex].HasResponded && players[playerBIndex].HasResponded) UpdatePhase(GamePhase.Voting);
            */
        }

        public void GiveVote(FactioPlayer player, bool voteIsB)
        {
            /*
            if (gamePhase != GamePhase.Voting) return;
            votes.RemoveAll((playerVote) => playerVote.Item1 == player);
            votes.Add((player, voteIsB));
            foreach (FactioPlayer p in players)
            {
                if (factioServer.IsDebugging) Program.LogLine(LoggingTag.FactioGame, "Voted: " + p.HasVoted, true);
                if (!p.HasVoted) return;
            }
            UpdatePhase(GamePhase.Results);
            */
        }
        #endregion PlayerInputMethods

        #region GenericMethods
        public void CloseLobby(LobbyClose reason)
        {
            CloseLobbyCPacket closeLobby = new CloseLobbyCPacket
            { Reason = (byte)reason };
            players.ForEach((p) =>
            GetNetPeer(p).Send(factioServer.listener.packetProcessor.Write(closeLobby), DeliveryMethod.ReliableOrdered));
            Program.LogLine(LoggingTag.FactioGame, $"Lobby closed, led by \"{players[0].username}\"");
            factioServer.gameManager.CloseLobby(this);
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
        private short GetRandomPlayerIndex()
        {
            return (short)factioServer.rand.Next(0, players.Count);
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
