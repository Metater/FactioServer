using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;
using FactioShared;

namespace FactioServer
{
    public class FactioServerListener : INetEventListener
    {
        private FactioServer factioServer;

        public NetPacketProcessor packetProcessor = new NetPacketProcessor();

        public FactioServerListener(FactioServer factioServer)
        {
            this.factioServer = factioServer;
            packetProcessor.SubscribeReusable<CreateLobbySPacket, NetPeer>(OnCreateLobbySPacketReceived);
            packetProcessor.SubscribeReusable<JoinLobbySPacket, NetPeer>(OnJoinLobbySPacketReceived);
            packetProcessor.SubscribeReusable<ReadySPacket, NetPeer>(OnReadySPacketReceived);
            packetProcessor.SubscribeReusable<ResponseSPacket, NetPeer>(OnResponseSPacketReceived);
            packetProcessor.SubscribeReusable<VoteSPacket, NetPeer>(OnVoteSPacketReceived);
            packetProcessor.SubscribeReusable<ServerCommandSPacket, NetPeer>(OnServerCommandSPacketReceived);
        }

        #region NetworkEvents
        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (factioServer.server.ConnectedPeersCount < 256)
                request.AcceptIfKey("Factio");
            else
                request.Reject();
        }
        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {

        }
        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {

        }
        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            packetProcessor.ReadAllPackets(reader, peer);
            reader.Recycle();
        }
        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {

        }
        public void OnPeerConnected(NetPeer peer)
        {
            FactioPlayer player = factioServer.PeerConnected(peer);
            Console.WriteLine($"[Server (Client Connected)] Client connected, assigned id {player.clientId}");
        }
        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            int clientId = factioServer.peerClientIdMap.GetClientId(peer);
            factioServer.PeerDisconnected(peer);
            Console.WriteLine($"[Server (Client Disconnected)] Client disconnected, with id {clientId}");
        }
        #endregion NetworkEvents

        #region ReceivedPacketImplementation
        private void OnCreateLobbySPacketReceived(CreateLobbySPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            player.username = packet.Username;
            if (factioServer.gameManager.TryCreateLobby(peer, player))
            {
                Console.WriteLine($"[Action (Created Lobby)] Client {player.clientId} named \"{player.username}\" made a lobby");
            }
        }
        private void OnJoinLobbySPacketReceived(JoinLobbySPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            player.username = packet.Username;
            if (factioServer.gameManager.TryJoinLobby(peer, player, packet.JoinCode))
            {
                Console.WriteLine($"[Action (Joined Lobby)] Client {player.clientId} named \"{player.username}\" joined a lobby");
            }
        }
        private void OnReadySPacketReceived(ReadySPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            if (packet.Value)
                Console.WriteLine($"[Action (Ready)] Client {player.clientId} named \"{player.username}\" is ready");
            else
                Console.WriteLine($"[Action (Unready)] Client {player.clientId} named \"{player.username}\" is not ready");
            player.Ready(packet.Value);
        }
        private void OnResponseSPacketReceived(ResponseSPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            player.Respond(packet.Response);
        }
        private void OnVoteSPacketReceived(VoteSPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            player.Vote(packet.VoteIsB);
        }
        private void OnServerCommandSPacketReceived(ServerCommandSPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            if (packet.password == factioServer.configRegistry.GetIntConfig("password"))
            {
                Console.WriteLine($"[Action (Client Command Execute)] Client with id {player.clientId}, named \"{player.username}\" is executing a command: {packet.command}");
                factioServer.commandHandler.Handle(packet.command);
            }
            else
            {
                Console.WriteLine($"[Action (Client Command Execute)] Client with id {player.clientId}, named \"{player.username}\" attempted to execute a command: {packet.command}");
            }
        }
        #endregion ReceivedPacketImplementation
    }
}
