using Data.Entity;

namespace Data.Interfaces
{
    public interface IServiceContext
    {
        public Task<IEnumerable<Service>> GetAllServices();

        public Task<bool> Create(string userId, Service service);

        public Task<bool> Update(string userId, Service service);
    }
}
