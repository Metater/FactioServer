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

        public bool TryCreateLobby(FactioPlayer leader)
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
            FactioGame game = new FactioGame(leader);
            games.Add(joinCode, game);
            leader.game = game;
            JoinedLobby(factioServer.peerClientIdMap.GetPeer(leader.clientId), joinCode);
            UpdatePlayersInGame(game);
            return true;
        }

        public bool TryJoinLobby(FactioPlayer player, int joinCode)
        {
            if (player.InGame) return false;
            if (games.TryGetValue(joinCode, out FactioGame game))
            {
                if (game.TryJoinGame(player))
                {
                    player.game = game;
                    JoinedLobby(factioServer.peerClientIdMap.GetPeer(player.clientId), joinCode);
                    UpdatePlayersInGame(game);
                    return true;
                }
            }
            return false;
        }

        private void JoinedLobby(NetPeer peer, int joinCode)
        {
            JoinedLobbyCPacket joinedLobby = new JoinedLobbyCPacket
            { JoinCode = joinCode };
            peer.Send(factioServer.listener.packetProcessor.Write(joinedLobby), DeliveryMethod.ReliableOrdered);
        }

        private void UpdatePlayersInGame(FactioGame game)
        {
            PlayerUpdateCPacket playerUpdate = new PlayerUpdateCPacket
            { Usernames = game.GetUsernames() };
            foreach (FactioPlayer player in game.players)
            {
                NetPeer peer = factioServer.peerClientIdMap.GetPeer(player.clientId);
                peer.Send(factioServer.listener.packetProcessor.Write(playerUpdate), DeliveryMethod.ReliableOrdered);
            }
        }

    }
}
