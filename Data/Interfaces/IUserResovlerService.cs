using Data.Entity;
using Microsoft.AspNetCore.Components.Authorization;

namespace Data.Interfaces
{
    public interface IUserResolverService
    {
        public AuthenticationStateProvider GetAuthenticationStateProvider();

        public Task<ApplicationUser?> GetCurrentUserAsync();

        public string? GetUserId();
    }
}
