using System.Security.Claims;
using AppModels.Login;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace AppAuth;

public class UserAuthStateProvider(
    ProtectedSessionStorage sessionStorage,
    ProtectedLocalStorage localStorage) : AuthenticationStateProvider
{
    private const string Key = "UserSession";
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private AuthenticationState? _cached;

    public UserSession? CurrentUser { get; private set; }
    public UserProfile? userProfile { get; set; }

    public Task<AuthenticationState> GetAuthStateAsync() => GetAuthenticationStateAsync();
    public async Task<ClaimsPrincipal> GetUserAsync() => (await GetAuthenticationStateAsync()).User;
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cached is not null)
            return _cached;

        try
        {
            var session = (await sessionStorage.GetAsync<UserSession>(Key)).Value
                       ?? (await localStorage.GetAsync<UserSession>(Key)).Value;

            if (session is null)
                return _cached = Anonymous;

            Apply(session);
            return _cached!;
        }
        catch (InvalidOperationException) // prerendering: JS interop unavailable
        {
            return Anonymous; // not cached, so it retries after interactive render
        }
    }

    public async Task SignInAsync(UserProfile user)
    {
        var session = new UserSession(); ;
        session.UserID = user.UserID;
        session.DisplayName = user.DisplayName;
        session.Email = user.Email;
        session.PhoneNumber = user.PhoneNumber;
        session.Designation = user.Designation;
        session.UserRole = user.UserRole;
        session.Session = user.Session;
        session.RememberMe = user.RememberMe;
        session.LoginTime = user.LoginTime;
   
        if (session.RememberMe)
            await localStorage.SetAsync(Key, session);
        else
            await sessionStorage.SetAsync(Key, session);

        Apply(session);
        NotifyAuthenticationStateChanged(Task.FromResult(_cached!));
    }

    public async Task SignOutAsync()
    {
        await sessionStorage.DeleteAsync(Key);
        await localStorage.DeleteAsync(Key);
        CurrentUser = null;
        _cached = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(_cached));
    }

    private void Apply(UserSession s)
    {
        CurrentUser = s;
        _cached = new AuthenticationState(CreatePrincipal(s));
    }

    private static ClaimsPrincipal CreatePrincipal(UserSession s)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, s.UserID),
            new(ClaimTypes.Name, s.DisplayName),
            new(ClaimTypes.Email, s.Email),
            new(ClaimTypes.Role, s.UserRole),
            new(ClaimTypes.MobilePhone, s.PhoneNumber),
            new("Designation", s.Designation),
            new("LastLogin", s.LoginTime.ToString("o")),
            new("Session", s.Session.ToString()),
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));
    }
}