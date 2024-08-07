namespace Data.Entity
{
    public abstract class BusinessControlledEntity : BaseEntity
    {
        public int BusinessId { get; set; }

        public Business Business { get; set; }

        public Guid GetBusinessGuid() => Business.Guid;
    }
}
