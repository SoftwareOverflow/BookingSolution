using Data.Entity;
using Data.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Auth.Components.Account
{
    // This is a server-side AuthenticationStateProvider that revalidates the security stamp for the connected user
    // every 30 minutes an interactive circuit is connected.
    internal sealed class IdentityRevalidatingAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider, IUserResolverService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IOptions<IdentityOptions> options;

        private string? UserId = null;

        public IdentityRevalidatingAuthenticationStateProvider(
                ILoggerFactory loggerFactory,
                IServiceScopeFactory scopeFactory,
                IOptions<IdentityOptions> options) : base(loggerFactory)
        {
            this.scopeFactory = scopeFactory;
            this.options = options;

            AuthenticationStateChanged += IdentityRevalidatingAuthenticationStateProvider_AuthenticationStateChanged;
        }

        private async void IdentityRevalidatingAuthenticationStateProvider_AuthenticationStateChanged(Task<AuthenticationState> task)
        {
            var state = await task;
            SetUserIdFromState(state);
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);
        

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            //Custom Code
            //AuthenticationStateChanged += IdentityRevalidatingAuthenticationStateProvider_AuthenticationStateChanged;
            //End Custom Code

            // Get the user manager from a new scope to ensure it fetches fresh data
            await using var scope = scopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            return await ValidateSecurityStampAsync(userManager, authenticationState.User);
        }

        private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return false;
            }
            else if (!userManager.SupportsUserSecurityStamp)
            {
                return true;
            }
            else
            {
                var principalStamp = principal.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType);
                var userStamp = await userManager.GetSecurityStampAsync(user);
                return principalStamp == userStamp;
            }
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var state = await GetAuthenticationStateAsync();
            SetUserIdFromState(state);

            if(UserId == null)
            {
                return null;
            } else
            {
                return new ApplicationUser
                {
                    Id = UserId
                };
            }
        }

        private void SetUserIdFromState(AuthenticationState state)
        {
            var user = state.User.Identity;
            if (user?.IsAuthenticated ?? false)
            {
                var newUserId = state.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (newUserId != null)
                {
                    UserId = newUserId;
                    Console.WriteLine("AuthStateProvider: " + newUserId);
                }
            }
            else
            {
                UserId = null;
            }
        }

        public string? GetUserId() => UserId;

        public AuthenticationStateProvider GetAuthenticationStateProvider() => this;
    }
}
