namespace Data.Entity
{
    public class BusinessEntity : BaseEntity
    {
        public string Name { get; set; }

        public Address Address { get; set; }

        public ICollection<Service> Services { get; set; }
    }
}
