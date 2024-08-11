using Microsoft.AspNetCore.Identity;

namespace Auth.Data
{
    internal class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";
    }
}