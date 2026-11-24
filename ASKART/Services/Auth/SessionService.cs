using Askart.Models;

namespace Askart.Services;

public enum StartDestination { Chats, Administration }

public sealed class SessionService
{
    public User? CurrentUser { get; private set; }
    public event Action? Cleared;

    public void SignIn(User user) => CurrentUser = user;
    public void Clear()
    {
        CurrentUser = null;
        Cleared?.Invoke();
    }

    public static StartDestination DestinationFor(User user) =>
        user.IsAdmin ? StartDestination.Administration : StartDestination.Chats;
}
