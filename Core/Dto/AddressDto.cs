using System.ComponentModel.DataAnnotations;

namespace Core.Dto
{
    public record AddressDto : DtoBase
    {
        [Required]
        public string Address1 { get; set; } = string.Empty;
        
        [Required]
        public string Address2 { get; set; } = string.Empty;


        public string Address3 { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        [Required]
        public string PostalCode { get; set; } = string.Empty;
    }
}
