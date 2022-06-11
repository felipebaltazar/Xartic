namespace Martic.Abstractions.Services
{
    public interface IXarticService
    {
        bool IsConnected { get; }

        Task OpenConnectionAsync(string username, string roomName);

        Task CloseConnectionAsync();

        Task SendMessage(string message);

        Task CheckRoomStatusAsync();

        void SubscribeFor(string commandName, Action action);

        void SubscribeFor<T>(string commandName, Action<T> action);

        void SubscribeFor<T1, T2>(string commandName, Action<T1, T2> action);
    }
}
