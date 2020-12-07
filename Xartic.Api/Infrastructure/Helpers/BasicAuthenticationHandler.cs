using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Xartic.Api.Infrastructure
{
    //Exemplo de autenticação usando o schema Basic
    internal class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string AUTHORIZATION = "Authorization";

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        { 
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Path.HasValue && Request.Path.Value.Contains("Xartic"))
            {
                if (!Request.Headers.ContainsKey(AUTHORIZATION))
                    return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

                try
                {
                    var value = Request.Headers[AUTHORIZATION];
                    var authHeader = AuthenticationHeaderValue.Parse(value);
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');

                    var username = credentials[0];
                    var password = credentials[1];

                    var claims = new[] {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Name, username),
                    };

                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    Response.HttpContext.User = principal;
                    Request.HttpContext.Response.Headers.Add(AUTHORIZATION, value);
                    Request.HttpContext.User = principal;
                    Context.User = principal;

                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                catch
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
                }
            }

            return Task.FromResult(AuthenticateResult.NoResult());
        }

    }
}
