using Askart.Models;

namespace Askart.ViewModels;

public sealed record MemberItem(User User, string Role)
{
    public string Initial => string.IsNullOrEmpty(User.Name) ? "" : User.Name[..1].ToUpper();
}
