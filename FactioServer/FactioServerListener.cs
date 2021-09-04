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
        public NetManager server;

        public NetPacketProcessor packetProcessor = new NetPacketProcessor();

        public FactioServerListener(FactioServer factioServer)
        {
            this.factioServer = factioServer;
            packetProcessor.SubscribeReusable<CreateLobbySPacket, NetPeer>(OnCreateLobbySPacketReceived);
            packetProcessor.SubscribeReusable<JoinLobbySPacket, NetPeer>(OnJoinLobbySPacketReceived);
            packetProcessor.SubscribeReusable<ResponseSPacket, NetPeer>(OnResponseSPacketReceived);
            packetProcessor.SubscribeReusable<VoteSPacket, NetPeer>(OnVoteSPacketReceived);
        }

        #region NetworkEvents
        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (server.ConnectedPeersCount < 100)
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
            if (factioServer.gameManager.TryCreateLobby(player))
            {
                Console.WriteLine($"[Action (Created Lobby)] Client {player.clientId} named \"{player.username}\" made a lobby");
            }
        }
        private void OnJoinLobbySPacketReceived(JoinLobbySPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            player.username = packet.Username;
            if (factioServer.gameManager.TryJoinLobby(player, packet.JoinCode))
            {
                Console.WriteLine($"[Action (Joined Lobby)] Client {player.clientId} named \"{player.username}\" joined a lobby");
            }
        }
        private void OnResponseSPacketReceived(ResponseSPacket packet, NetPeer peer)
        {

        }
        private void OnVoteSPacketReceived(VoteSPacket packet, NetPeer peer)
        {

        }
        #endregion ReceivedPacketImplementation
    }
}
