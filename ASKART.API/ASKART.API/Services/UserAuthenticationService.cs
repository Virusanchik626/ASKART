using ASKART.API.Data;
using ASKART.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ASKART.API.Services;

public sealed class UserAuthenticationService(AskartContext context)
{
    public async Task<User?> AuthenticateAsync(string login, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u =>
            u.Login == login || u.Email == login || u.Number == login);
        return user != null && user.Password == password ? user : null;
    }
}
