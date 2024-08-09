using System.ComponentModel.DataAnnotations;

namespace Core.Dto
{
    public record BusinessDto : DtoBase
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Website")]
        public string Url { get; set; }

        [ValidateComplexType]
        public AddressDto Address { get; set; } = new AddressDto();

    }
}
