using Auth.Extensions;
using Microsoft.AspNetCore.Builder;

namespace Core.Extensions
{
    public static class WebApplicationExtensions
    {
        public static void AddApplicationAssemblies(this RazorComponentsEndpointConventionBuilder builder)
        {
            builder.AddAdditionalAssemblies(typeof(Auth.Components.Account.Pages.Login).Assembly);
        }

        public static void AddApplicationLayers(this WebApplication app)
        {
            app.AddAuthenticationLayer();
        }
    }
}
