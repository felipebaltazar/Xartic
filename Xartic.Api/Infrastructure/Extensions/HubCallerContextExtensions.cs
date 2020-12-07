using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Text;

namespace Xartic.Api.Extensions
{
    internal static class HubCallerContextExtensions
    {
        private const string AUTH_HEADER = "Authorization";
        private const string ISO_ENCODING = "ISO-8859-1";

        internal static string ToRoomName(this HubCallerContext context)
        {
            var httpCtx = context.GetHttpContext();
            var query = httpCtx.Request.Query;
            return query["roomName"];
        }

        internal static string ToUserName(this HubCallerContext context)
        {
            var httpCtx = context.GetHttpContext();
            var query = httpCtx.Request.Query;
            var username = query["username"];

            if (!string.IsNullOrWhiteSpace(username))
                return username;

            if (!httpCtx.Request.Headers.TryGetValue(AUTH_HEADER, out var auth)) return null;

            var token = auth.ToString().Split(" ");
            var encryptedToken = token.ElementAtOrDefault(1);

            if (string.IsNullOrWhiteSpace(encryptedToken)) return null;

            var authCredencials = Encoding.GetEncoding(ISO_ENCODING)
                .GetString(Convert.FromBase64String(encryptedToken));

            var credencial = authCredencials.Split(":");
            return credencial.FirstOrDefault();
        }
    }
}
