using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Dto
{
    public class PersonDto
    {
        [Required(ErrorMessage = "First name is required")]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [DisplayName("Email Address")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [DataType(DataType.PhoneNumber)]
        [DisplayName("Phone")]
        public string PhoneNumber { get; set; }
    }
}
