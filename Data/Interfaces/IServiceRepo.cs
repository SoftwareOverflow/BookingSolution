using Data.Entity;

namespace Data.Interfaces
{
    public interface IServiceRepo
    {
        public Task<IEnumerable<Service>> GetAllServicesForUser();

        public Task<bool> Create(string userId, Service service);

        public Task<bool> Update(string userId, Service service);

        public Task<bool> Delete(Guid serviceId);
    }
}
