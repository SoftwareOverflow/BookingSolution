namespace Data.Entity
{
    public class BusinessControlledEntity : BaseEntity
    {
        public int BusinessId { get; set; }

        public BusinessEntity Business { get; set; }

        public Guid GetBusinessGuid() => Business.Guid;
    }
}
