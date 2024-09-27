using Data.Entity;

namespace Data.Interfaces
{
    public interface IServiceRepo
    {
        public Task<IEnumerable<Service>> GetServices();

        public Task<bool> Create(Service service);

        public Task<bool> Update(Service service);

        public Task<bool> Delete(Guid serviceId);
    }
}
