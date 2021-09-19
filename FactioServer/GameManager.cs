using FactioShared;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace FactioServer
{
    public class GameManager : ITickable
    {
        private FactioServer factioServer;

        public Dictionary<int, FactioGame> games = new Dictionary<int, FactioGame>();

        public GameManager(FactioServer factioServer)
        {
            this.factioServer = factioServer;
        }

        public void Tick(long id)
        {
            foreach (KeyValuePair<int, FactioGame> game in games)
            {
                game.Value.Tick(id);
            }
        }

        public bool TryCreateLobby(NetPeer peer, FactioPlayer leader)
        {
            if (leader.InGame) return false;
            int joinCode = -1;
            int tries = 0;
            while (games.ContainsKey(joinCode) || tries == 0)
            {
                if (tries > 1000) throw new Exception("Need larger join code!");
                joinCode = factioServer.rand.Next(1000, 10000);
                tries++;
            }
            FactioGame game = new FactioGame(factioServer, joinCode, leader);
            games.Add(joinCode, game);
            leader.JoinLobby(game);
            JoinedLobby(peer, joinCode);
            UpdatePlayersInGame(game);
            return true;
        }

        public bool TryJoinLobby(NetPeer peer, FactioPlayer player, int joinCode)
        {
            if (player.InGame) return false;
            if (games.TryGetValue(joinCode, out FactioGame game))
            {
                if (game.TryJoinLobby(player))
                {
                    player.JoinLobby(game);
                    JoinedLobby(peer, joinCode);
                    UpdatePlayersInGame(game);
                    return true;
                }
            }
            return false;
        }

        public bool TryLeaveLobby(NetPeer peer, FactioPlayer player)
        {
            if (!player.InGame) return false;
            FactioGame game = player.Game;
            player.LeaveLobby();
            if (game.players.Count > 0) UpdatePlayersInGame(game);
            return true;
        }

        public bool CloseLobby(FactioGame game)
        {
            return games.Remove(game.JoinCode);
        }

        public void ServerShutdown()
        {
            foreach (KeyValuePair<int, FactioGame> game in games)
            {
                game.Value.CloseLobby(LobbyClose.ServerShutdown);
            }
        }

        public void JoinedLobby(NetPeer peer, int joinCode)
        {
            JoinedLobbyCPacket joinedLobby = new JoinedLobbyCPacket
            { JoinCode = joinCode };
            peer.Send(factioServer.listener.packetProcessor.Write(joinedLobby), DeliveryMethod.ReliableOrdered);
        }

        private void UpdatePlayersInGame(FactioGame game)
        {
            (int[], string[]) players = game.GetPlayers();
            PlayerUpdateCPacket playerUpdate = new PlayerUpdateCPacket
            { PlayerIds = players.Item1, Usernames = players.Item2 };
            foreach (FactioPlayer player in game.players)
            {
                NetPeer peer = factioServer.peerClientIdMap.GetPeer(player.clientId);
                peer.Send(factioServer.listener.packetProcessor.Write(playerUpdate), DeliveryMethod.ReliableOrdered);
            }
        }

    }
}
