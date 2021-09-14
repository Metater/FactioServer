using System;
using System.Collections.Generic;
using System.Text;

namespace FactioShared
{
    public enum LobbyClose : byte
    { 
        ServerShutdown,
        LeaderLeft,
        OnlyPlayer
    }
}
