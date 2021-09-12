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

        public bool isExitRequested = false;

        public bool IsDebugging { get; private set; } = false;
        public bool isDebuggingTicks = false;
        public int PollPeriod { get; private set; } = 1; // could add command for adjusting later

        public PeerClientIdMap peerClientIdMap = new PeerClientIdMap();
        public List<FactioPlayer> players = new List<FactioPlayer>();

        public Random rand = new Random();

        public long lastTick = 0;


        // bools use is, has can

        public FactioServer()
        {
            commandHandler = new CommandHandler(this);
            configRegistry = new ConfigRegistry(this);
            LoadServerConfig();

            listener = new FactioServerListener(this);
            server = new NetManager(listener);
            server.Start(12733);
            Console.WriteLine("[Factio Server] Server listening for connections on port 12733");

            gameManager = new GameManager(this);
            scenarioRegistry = new ScenarioRegistry(this);
        }

        public void LoadServerConfig()
        {
            commandHandler.OutputLine($"[Factio Server] Reloading server config");
            IsDebugging = configRegistry.GetBoolConfig("isDebugging");
            isDebuggingTicks = configRegistry.GetBoolConfig("isDebuggingTicks");
            PollPeriod = configRegistry.GetIntConfig("pollPeriod");
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
            // do important stuff
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
