using Data.Entity.Appointments;

namespace Data.Entity
{
    public class Business : BaseEntity
    {
        public string Name { get; set; }

        /// <summary>
        /// The URL for the business. This is the URL the API booking requests will come from
        /// </summary>
        public string Url { get; set; }

        
        public int AddressId { get; set; }
        
        // TODO might need some sort of API Key to check against, or could possibly use the guid property

        public virtual Address Address { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = [];

        public virtual ICollection<TimeBlock> TimeBlocks { get; set; } = [];

        public virtual ICollection<Service> Services { get; set; } = [];

        public virtual ICollection<BusinessUser> Users { get; set; } = [];
    }
}