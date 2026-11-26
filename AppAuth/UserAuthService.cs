using AppModels.Login;

namespace AppAuth;

public interface IUserAuthService
{
    public UserProfile? CurrentUser { get; }
    Task SignInAsync(UserProfile user);
    Task SignOutAsync();
}

public class UserAuthService(UserAuthStateProvider provider) : IUserAuthService
{
    public UserProfile? CurrentUser => provider.userProfile;



    public Task SignInAsync(UserProfile userProfile)
    {
        userProfile.Session = Guid.NewGuid();
        userProfile.LoginTime = DateTime.UtcNow;
        return provider.SignInAsync(userProfile);
    }

    public Task SignOutAsync() => provider.SignOutAsync();
}