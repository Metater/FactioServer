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
        public bool debugTicks = false;
        public int pollRate = 1; // could add command for adjusting later

        public PeerClientIdMap peerClientIdMap = new PeerClientIdMap();
        public List<FactioPlayer> players = new List<FactioPlayer>();

        public Random rand = new Random();

        public long lastTick = 0;

        public FactioServer()
        {
            listener = new FactioServerListener(this);
            server = new NetManager(listener);
            server.Start(12733);
            Console.WriteLine("[Core] Server listening for connections on port 12733");

            gameManager = new GameManager(this);
            scenarioRegistry = new ScenarioRegistry(this);
        }

        public void Tick(long id)
        {
            lastTick = id;
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
                    Console.WriteLine("\tdebug ticks: Toggles tick debugging");
                    Console.WriteLine("\tclear: Clears console");
                    break;
                case "debug ticks":
                    debugTicks = !debugTicks;
                    if (debugTicks)
                        Console.WriteLine("[Core] Tick debugging enabled.");
                    else
                        Console.WriteLine("[Core] Tick debugging disabled.");
                    break;
                case "clear":
                    Console.Clear();
                    break;
                default:
                    Console.WriteLine("[Core] Unknown command: " + command);
                    break;
            }
        }
    }
}
