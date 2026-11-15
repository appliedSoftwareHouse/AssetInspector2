using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Authorization;
using AppModels.Login;


namespace AppAuth
{
    public class UserAuthState
    {
        private readonly ProtectedSessionStorage _sessionStorage;
        private readonly ClaimsPrincipal AnyOne = new(new ClaimsIdentity());
        public IEnumerable<Claim> Claims { get; set; }
        public UserAuthModel LoginUser { get; set; } = new();


        public UserAuthState(ProtectedSessionStorage sessionStorage)
        {
            _sessionStorage = sessionStorage;
            Claims = [];
        }

        public async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var result = await _sessionStorage.GetAsync<UserAuthModel>("UserAuthModel");

                if (!result.Success || result.Value is null)
                    return new AuthenticationState(AnyOne);

                var principal = CreatePrincipal(result.Value);
                var state = new AuthenticationState(principal);

                GetAppUser(state);
                Claims = [.. principal.Claims];
                return state;
            }
            catch (InvalidOperationException) // prerendering / JS interop unavailable
            {
                return new AuthenticationState(AnyOne);
            }
        }


        private void GetAppUser(AuthenticationState authState)
        {
            var claims = authState.User.Claims.ToList();

            string GetClaim(string type) =>
                claims.FirstOrDefault(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase))?.Value ?? "";


            LoginUser.UserID = GetClaim(ClaimTypes.NameIdentifier);
            LoginUser.DisplayName = GetClaim(ClaimTypes.Name);
            LoginUser.Email = GetClaim(ClaimTypes.Email);
            LoginUser.Role = GetClaim(ClaimTypes.Role);
            LoginUser.Session = Guid.TryParse(GetClaim("Session"), out var session) ? session : Guid.Empty;
            LoginUser.LoginTime = DateTime.TryParse(GetClaim("LastLogin"), out var lastLogin) ? lastLogin : DateTime.MinValue;

        }

        private static ClaimsPrincipal CreatePrincipal(UserAuthModel user)
        {
            var Claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.UserID),
                new (ClaimTypes.Name, user.DisplayName),
                new (ClaimTypes.Role, user.Role),
                new (ClaimTypes.Email, user.Email),
                new ("LastLogin", user.LoginTime.ToString("o")),
                new ("Session", user.Session.ToString())

            };

            return new ClaimsPrincipal(new ClaimsIdentity(Claims, "CustomAuth"));
        }
    }
}
