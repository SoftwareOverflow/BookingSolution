using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entity
{
    public class BusinessUser
    {
        [Key, ForeignKey("AspNetUsers")]
        public string UserId { get; set; }

        public int BusinessId { get; set; }

        public virtual IdentityUser User { get; set; }

        public virtual Business Business { get; set; }


    }
}
