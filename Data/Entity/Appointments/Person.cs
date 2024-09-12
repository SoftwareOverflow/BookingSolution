using System.ComponentModel.DataAnnotations;

namespace Data.Entity.Appointments
{
    public class Person : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(320)]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(50)]
        public string PhoneNumber { get; set; }
    }
}
