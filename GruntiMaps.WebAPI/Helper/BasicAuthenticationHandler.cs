using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GruntiMaps.Api.Common.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GruntiMaps.WebAPI.Helper
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly string _adminToken;
        private const string AccessTokenHeader = "Authorization";

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IOptions<AuthOptions> authOptions)
            : base(options, logger, encoder, clock)
        {
            _adminToken = authOptions.Value.AdminToken;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(new Claim[0], Scheme.Name)), Scheme.Name);

            if (string.IsNullOrEmpty(_adminToken))
            {
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            if (!Request.Headers.ContainsKey(AccessTokenHeader))
            {
                return Task.FromResult(AuthenticateResult.Fail($"Missing {AccessTokenHeader} Header"));
            }

            try
            {
                var credential = AuthenticationHeaderValue.Parse(Request.Headers[AccessTokenHeader]).Parameter;
                return Task.FromResult(credential == _adminToken
                    ? AuthenticateResult.Success(ticket)
                    : AuthenticateResult.Fail($"Invalid {AccessTokenHeader}"));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail($"Invalid {AccessTokenHeader} Header"));
            }
        }
    }
}
