using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Dto
{
    public class PersonDto
    {
        [Required(ErrorMessage = "First name is required")]
        [DisplayName("First Name")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [DisplayName("Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [DisplayName("Email Address")]
        [DataType(DataType.EmailAddress)]
        [MaxLength(320)]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [DataType(DataType.PhoneNumber)]
        [DisplayName("Phone")]
        [MaxLength(50)]
        public string PhoneNumber { get; set; }
    }
}
