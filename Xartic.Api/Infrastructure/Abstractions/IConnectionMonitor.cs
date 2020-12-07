using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SignalHub = Microsoft.AspNetCore.SignalR.Hub;

namespace Xartic.Api.Abstractions
{
    public interface IConnectionMonitor<THub> where THub : SignalHub
    {
        /// <summary>
        /// Inicializa o monitoramento das conexões com o hub
        /// </summary>
        /// <param name="context"></param>
        Task InitConnectionMonitoringAsync(HubCallerContext context, THub hub);

        /// <summary>
        /// Localiza o id de conexão à partir de um nome de usuário conectado
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        IEnumerable<string> GetByGroup(string groupName);

        /// <summary>
        /// Localiza todos os usuários conectados em uma específica sala
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        string GetId(string username);

        /// <summary>
        /// Monitora o fim da conexão
        /// </summary>
        /// <param name="hub">Hub onde o cliente foi desconectado</param>
        /// <param name="exception">Exception (caso tenha ocorrido)</param>
        /// <returns></returns>
        bool CloseConnection(string connectionId);

        /// <summary>
        /// Desconecta um usuário à partir do servidor
        /// </summary>
        /// <param name="userName">nome de usuário à ser desconectado</param>
        /// <returns></returns>
        Task OnDisconnectedAsync(THub hub, Exception exception);
    }
}
