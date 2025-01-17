﻿using Auth.Components.Account;
using Auth.Data;
using Auth.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Auth.Services
{
    internal class UserService(
        ILogger<UserService> Logger,
        SignInManager<ApplicationUser> SignInManager,
        IHttpContextAccessor contextAccessor,
        UserManager<ApplicationUser> UserManager,
        IdentityRedirectManager RedirectManager) : IUserServiceInternal, IUserService
    {


        private List<IUserObserver> Observers = [];

        private ClaimsPrincipal? User => contextAccessor.HttpContext?.User ?? null;

        public void AddUserListener(IUserObserver observer)
        {
            Observers.Add(observer);
        }

        public string GetCurrentUserId()
        {
            if (User != null)
            {
                return UserManager.GetUserId(User) ?? string.Empty;
            }

            return string.Empty;
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            if (User != null)
            {
                var result = await UserManager.GetUserAsync(User);
                return result?.Id ?? string.Empty;
            }

            return string.Empty;


        }

        public async Task<string> GetUserNameFromId(string userId)
        {
            if (User != null)
            {
                var result = await UserManager.GetUserAsync(User);
                if (result?.Id == userId)
                {
                    return result.FirstName;
                }

                throw new UnauthorizedAccessException($"The supplied UserId {userId} does not match the currently logged in user");
            }

            return string.Empty;
        }

        public void NotifyUserEvent(UserEvent userEvent, string userId)
        {
            Observers.ForEach(x =>
            {
                x.OnUserEvent(userEvent, userId);
            });
        }

        public void RemoveUserListener(IUserObserver observer)
        {
            Observers.Remove(observer);
        }

        public string GetSignOutPage() => "/Account/Logout";
        //{
        //    RedirectManager.RedirectTo("/Account/Logout");

        //    await Task.Delay(200);

        //    /*var context = contextAccessor.HttpContext;
        //    if(context != null)
        //    {
        //        var baseAddress = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}";
        //        Uri.TryCreate(baseAddress, UriKind.Absolute, out var uri);
        //        httpClient.BaseAddress = uri;
        //        await httpClient.PostAsync("Account/Logout", null);
        //    }*/
        //    //await signInManager.SignOutAsync();
        //}
    }
}
