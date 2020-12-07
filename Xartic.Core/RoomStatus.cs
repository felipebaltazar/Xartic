using Newtonsoft.Json;
using System.Collections.Generic;

namespace Xartic.Core
{
    public class RoomStatus
    {
        [JsonProperty("players")]
        public IEnumerable<Player> Players { get; set; }

        [JsonProperty("roomName")]
        public string RoomName { get; set; }

        [JsonProperty("winner")]
        public Player Winner { get; set; }

        public bool HasWinner() => Winner != null;

        public static RoomStatus Build(string roomName, IEnumerable<Player> players, Player winner = null)
        {
            return new RoomStatus()
            {
                RoomName = roomName,
                Players = players,
                Winner = winner
            };
        }
    }

    public class Player
    {
        [JsonConstructor]
        public Player()
        {
        }

        public Player(string username)
        {
            Username = username;
        }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}
