using System.Collections.Generic;
using Xartic.Api.Domain.Models;
using Xartic.Core;

namespace Xartic.Api.Infrastructure.Abstractions
{
    public interface IGameController
    {
        /// <summary>
        /// Get the current room draw
        /// </summary>
        /// <returns></returns>
        IEnumerable<DrawCommand> GetCurrentDraw();

        /// <summary>
        /// Occours on every new draw
        /// </summary>
        /// <param name="drawCommand"></param>
        void OnDrawReceived(DrawCommand drawCommand);

        /// <summary>
        /// Occours when draws are cleared
        /// </summary>
        void OnClearReceived();

        /// <summary>
        /// Checks for response result
        /// </summary>
        /// <param name="response">Player response attempt</param>
        /// <returns>Reponse result</returns>
        ResponseResult OnResponse(string response);

        /// <summary>
        /// Initialize the game
        /// </summary>
        /// <param name="roomName"></param>
        /// <returns></returns>
        string StartGame(string roomName, string host);

        /// <summary>
        /// Check game status on player lost connection
        /// </summary>
        /// <param name="connectionId">Connection Id</param>
        /// <returns>true if game is stared</returns>
        bool OnPlayerDisconnected(string connectionId);
    }
}
