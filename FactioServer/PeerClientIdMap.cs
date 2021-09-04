using System;
using System.Collections.Generic;
using System.Text;
using LiteNetLib;

namespace FactioServer
{
    public class PeerClientIdMap
    {
        private List<(NetPeer, int)> peerClientIdMap = new List<(NetPeer, int)>();
        private int nextClientId = 0;

        public int AddPeer(NetPeer peer)
        {
            peerClientIdMap.Add((peer, nextClientId));
            nextClientId++;
            return nextClientId - 1;
        }

        public int GetClientId(NetPeer peer)
        {
            foreach ((NetPeer, int) peerClientId in peerClientIdMap)
            {
                if (peerClientId.Item1 == peer)
                    return peerClientId.Item2;
            }
            return -1;
        }
        public NetPeer GetPeer(int clientId)
        {
            foreach ((NetPeer, int) peerClientId in peerClientIdMap)
            {
                if (peerClientId.Item2 == clientId)
                    return peerClientId.Item1;
            }
            return null;
        }

        public void RemoveClientId(int clientId)
        {
            for (int i = 0; i < peerClientIdMap.Count; i++)
            {
                (NetPeer, int) peerClientId = peerClientIdMap[i];
                if (peerClientId.Item2 == clientId)
                {
                    peerClientIdMap.Remove(peerClientId);
                    break;
                }
            }
        }
        public void RemovePeer(NetPeer peer)
        {
            for (int i = 0; i < peerClientIdMap.Count; i++)
            {
                (NetPeer, int) peerClientId = peerClientIdMap[i];
                if (peerClientId.Item1 == peer)
                {
                    peerClientIdMap.Remove(peerClientId);
                    break;
                }
            }
        }
    }
}
