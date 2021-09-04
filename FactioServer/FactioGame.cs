using System;
using System.Collections.Generic;
using System.Text;

namespace FactioServer
{
    public class FactioGame : ITickable
    {
        public List<FactioPlayer> players = new List<FactioPlayer>();

        public bool gameStarted = false;
        // option to make game public later
        // chat later

        public FactioGame(FactioPlayer leader)
        {
            players.Add(leader);
        }

        public void Tick(long id)
        {

        }

        public bool TryJoinGame(FactioPlayer player)
        {
            if (gameStarted) return false;
            if (players.Contains(player)) return false;
            players.Add(player);
            if (players.Exists((p) => p.username == player.username))
            {
                int number = 2;
                string username = player.username + number;
                while (players.Exists((p) => p.username == username))
                {
                    number++;
                    username = player.username + number;
                }
                player.username = username;
            }
            return true;
        }

        public string[] GetUsernames()
        {
            List<string> usernames = new List<string>();
            foreach (FactioPlayer player in players)
            {
                usernames.Add(player.username);
            }
            return usernames.ToArray();
        }
    }
}
