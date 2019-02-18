/*

Copyright 2016, 2017, 2018 GIS People Pty Ltd

This file is part of GruntiMaps.

GruntiMaps is free software: you can redistribute it and/or modify it under 
the terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

GruntiMaps is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE. See the GNU Affero General Public License for more 
details.

You should have received a copy of the GNU Affero General Public License along
with GruntiMaps.  If not, see <https://www.gnu.org/licenses/>.

*/
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
