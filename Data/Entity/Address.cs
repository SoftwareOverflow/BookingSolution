using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entity
{
    public class Address : BaseEntity
    {
        public string Address1 { get; set; } = string.Empty;
        
        public string Address2 { get; set; } = string.Empty;

        public string Address3 { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
    }
}
