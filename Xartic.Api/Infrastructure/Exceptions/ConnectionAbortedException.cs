using System;

namespace Xartic.Api.Infrastructure.Exceptions
{
    public sealed class ConnectionAbortedException : Exception
    {
        public string HubName { get;}

        public ConnectionAbortedException(string hubName)
        {
            HubName = hubName;
        }

        public ConnectionAbortedException(string hubName, string message) : base(message)
        {
            HubName = hubName;
        }

        public ConnectionAbortedException(string hubName, string message, Exception innerException) : base(message, innerException)
        {
            HubName = hubName;
        }
    }
}
