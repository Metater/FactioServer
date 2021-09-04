using System;
using System.Collections.Generic;
using System.Text;

namespace FactioServer
{
    public class FactioPlayer
    {
        public int clientId;
        public string username = "";
        public bool InGame => game != null;
        public FactioGame game;

        public FactioPlayer(int clientId)
        {
            this.clientId = clientId;
        }
    }
}
