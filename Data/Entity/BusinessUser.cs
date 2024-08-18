using System.ComponentModel.DataAnnotations;

namespace Data.Entity
{
    public class BusinessUser
    {
        [Key]
        public string UserId { get; set; }

        public int BusinessId { get; set; }

        public Business Business { get; set; }
    }
}
