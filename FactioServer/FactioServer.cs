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
        public int PollPeriod { get; private set; } = 1;

        public PeerClientIdMap peerClientIdMap = new();
        public List<FactioPlayer> players = new();

        public Random rand = new();

        public long lastTick = 0;

        public FactioServer()
        {
            commandHandler = new CommandHandler(this);
            configRegistry = new ConfigRegistry(this);
            LoadServerConfig();
            scenarioRegistry = new ScenarioRegistry(this);

            listener = new FactioServerListener(this);
            server = new NetManager(listener);
            server.Start(12733);
            Program.LogLine(LoggingTag.FactioServer, "Server listening for connections on port 12733");

            gameManager = new GameManager(this);
        }

        public void LoadServerConfig()
        {
            commandHandler.OutputLine(LoggingTag.FactioServer, "Loading server config");
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
            FactioPlayer player = new(clientId);
            players.Add(player);
            return player;
        }

        public void PeerDisconnected(NetPeer peer)
        {
            FactioPlayer player = GetPlayer(peer);
            player.LeaveLobby();
        }

        public FactioPlayer GetPlayer(int clientId)
        {
            return players.Find((p) => p.clientId == clientId);
        }
        public FactioPlayer GetPlayer(NetPeer peer)
        {
            return GetPlayer(peerClientIdMap.GetClientId(peer));
        }

        public void ServerShutdown()
        {
            gameManager.ServerShutdown();
        }
    }
}
