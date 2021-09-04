using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;

namespace FactioServer
{
    public class FactioServer : ITickable
    {
        public FactioServerListener listener;
        public NetManager server;
        public GameManager gameManager;
        public ScenarioRegistry scenarioRegistry;

        public bool requestedExit = false;

        public PeerClientIdMap peerClientIdMap = new PeerClientIdMap();
        public List<FactioPlayer> players = new List<FactioPlayer>();

        public Random rand = new Random();

        public FactioServer()
        {
            listener = new FactioServerListener(this);
            server = new NetManager(listener);
            listener.server = server;
            server.Start(12733);
            Console.WriteLine("[Core] Server listening for connections on port 12733");

            gameManager = new GameManager(this);
            scenarioRegistry = new ScenarioRegistry();
        }

        public void Tick(long id)
        {
            gameManager.Tick(id);
        }

        public FactioPlayer PeerConnected(NetPeer peer)
        {
            int clientId = peerClientIdMap.AddPeer(peer);
            FactioPlayer player = new FactioPlayer(clientId);
            players.Add(player);
            return player;
        }

        public void PeerDisconnected(NetPeer peer)
        {

        }

        public FactioPlayer GetPlayer(int clientId)
        {
            return players.Find((p) => p.clientId == clientId);
        }
        public FactioPlayer GetPlayer(NetPeer peer)
        {
            return GetPlayer(peerClientIdMap.GetClientId(peer));
        }

        public void Command(string command)
        {
            switch (command)
            {
                case "exit":
                    requestedExit = true;
                    Console.WriteLine("[Core] Exiting.");
                    break;
                case "help":
                    Console.WriteLine("[Core] Commands: ");
                    Console.WriteLine("\texit: Closes the server");
                    break;
                default:
                    Console.WriteLine("[Core] Unknown command: " + command);
                    break;
            }
        }
    }
}
