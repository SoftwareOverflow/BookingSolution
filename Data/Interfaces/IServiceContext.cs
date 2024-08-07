using Data.Entity;

namespace Data.Interfaces
{
    public interface IServiceContext
    {
        public Task<IEnumerable<Service>> GetAllServices();

    }
}
