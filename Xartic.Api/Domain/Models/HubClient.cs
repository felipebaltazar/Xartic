namespace Xartic.Api.Domain.Models
{
    internal sealed class HubClient
    {
        public HubClient(string connectionId, string userName, string roomName)
        {
            ConnectionId = connectionId;
            Username = userName;
            RoomName = roomName;
        }

        public string ConnectionId { get; }

        public string Username { get; }

        public string RoomName { get; }
    }
}
