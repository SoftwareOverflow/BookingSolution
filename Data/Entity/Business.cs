namespace Data.Entity
{
    public class Business : BaseEntity
    {
        public string Name { get; set; }

        public virtual Address Address { get; set; }

        public virtual ICollection<Service> Services { get; set; }
    }
}
