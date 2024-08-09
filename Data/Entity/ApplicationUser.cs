using Microsoft.AspNetCore.Identity;

namespace Data.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? BusinessId { get; set; }
        public virtual Business? Business { get; set; }
    }
}
