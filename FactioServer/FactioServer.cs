using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;

namespace FactioServer
{
    public class FactioServer : ITickable
    {
        public ConfigRegistry configRegistry;
        public FactioServerListener listener;
        public NetManager server;
        public GameManager gameManager;
        public ScenarioRegistry scenarioRegistry;
        public CommandHandler commandHandler;

        public bool requestedExit = false;
        public bool debugTicks = false;
        public int pollPeriod = 1; // could add command for adjusting later

        public PeerClientIdMap peerClientIdMap = new PeerClientIdMap();
        public List<FactioPlayer> players = new List<FactioPlayer>();

        public Random rand = new Random();

        public long lastTick = 0;

        public FactioServer()
        {
            configRegistry = new ConfigRegistry(this);

            listener = new FactioServerListener(this);
            server = new NetManager(listener);
            server.Start(12733);
            Console.WriteLine("[Core] Server listening for connections on port 12733");

            gameManager = new GameManager(this);
            scenarioRegistry = new ScenarioRegistry(this);
            commandHandler = new CommandHandler(this);
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
    }
}
