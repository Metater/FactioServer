using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LiteNetLib;
using LiteNetLib.Utils;
using FactioShared;
using System.Text.RegularExpressions;

namespace FactioServer
{
    public class FactioServerListener : INetEventListener
    {
        private readonly FactioServer factioServer;

        public NetPacketProcessor packetProcessor = new();

        public FactioServerListener(FactioServer factioServer)
        {
            this.factioServer = factioServer;
            packetProcessor.SubscribeReusable<CreateLobbySPacket, NetPeer>(OnCreateLobbySPacketReceived);
            packetProcessor.SubscribeReusable<JoinLobbySPacket, NetPeer>(OnJoinLobbySPacketReceived);
            packetProcessor.SubscribeReusable<LeaveLobbySPacket, NetPeer>(OnLeaveLobbySPacketReceived);
            packetProcessor.SubscribeReusable<ReadySPacket, NetPeer>(OnReadySPacketReceived);
            packetProcessor.SubscribeReusable<ResponseSPacket, NetPeer>(OnResponseSPacketReceived);
            packetProcessor.SubscribeReusable<VoteSPacket, NetPeer>(OnVoteSPacketReceived);
            packetProcessor.SubscribeReusable<ServerCommandSPacket, NetPeer>(OnServerCommandSPacketReceived);
        }

        #region NetworkEvents
        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (factioServer.server.ConnectedPeersCount < 100)
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
            Program.LogLine(LoggingTag.FactioServerListener, $"Client connected, assigned id {player.clientId}");
        }
        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            int clientId = factioServer.peerClientIdMap.GetClientId(peer);
            factioServer.PeerDisconnected(peer);
            Program.LogLine(LoggingTag.FactioServerListener, $"Client disconnected, with id {clientId}");
        }
        #endregion NetworkEvents

        #region ReceivedPacketImplementation
        private void OnCreateLobbySPacketReceived(CreateLobbySPacket packet, NetPeer peer)
        {
            if (!NameValid(packet.Username)) return;
            FactioPlayer player = factioServer.GetPlayer(peer);
            player.username = packet.Username;
            if (factioServer.gameManager.TryCreateLobby(peer, player))
            {
                Program.LogLine(LoggingTag.FactioServerListener, $"Client named {player} made a lobby");
            }
        }
        private void OnJoinLobbySPacketReceived(JoinLobbySPacket packet, NetPeer peer)
        {
            if (!NameValid(packet.Username)) return;
            FactioPlayer player = factioServer.GetPlayer(peer);
            player.username = packet.Username;
            if (factioServer.gameManager.TryJoinLobby(peer, player, packet.JoinCode))
            {
                Program.LogLine(LoggingTag.FactioServerListener, $"Client named {player} joined a lobby");
            }
        }
        private void OnLeaveLobbySPacketReceived(LeaveLobbySPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            if (factioServer.gameManager.TryLeaveLobby(player))
            {
                Program.LogLine(LoggingTag.FactioServerListener, $"Client named {player} left a lobby");
            }
        }
        private void OnReadySPacketReceived(ReadySPacket packet, NetPeer peer)
        {
            FactioPlayer player = factioServer.GetPlayer(peer);
            if (packet.Value)
                Program.LogLine(LoggingTag.FactioServerListener, $"Client named {player} is ready");
            else
                Program.LogLine(LoggingTag.FactioServerListener, $"Client named {player} is not ready");
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
                Program.LogLine(LoggingTag.FactioServerListener, $"Client named {player} and ip {peer.EndPoint.Address} is executing a command: {packet.command}");
                string output = factioServer.commandHandler.Handle(packet.command, true);
                ServerMessageCPacket serverMessage = new()
                { Message = output };
                peer.Send(packetProcessor.Write(serverMessage), DeliveryMethod.ReliableOrdered);
            }
            else
            {
                Program.LogLine(LoggingTag.FactioServerListener, $"Client named {player} and ip {peer.EndPoint.Address} attempted to execute a command: {packet.command}");
                ServerMessageCPacket serverMessage = new()
                { Message = "[Factio Server] Incorrect password" };
                peer.Send(packetProcessor.Write(serverMessage), DeliveryMethod.ReliableOrdered);
            }
        }
        #endregion ReceivedPacketImplementation

        #region HelperMethods
        private static bool NameValid(string name)
        {
            Match match = Regex.Match(name, "<.*?>");
            return !match.Success;
        }
        #endregion HelperMethods
    }
}
