namespace Core.Interfaces
{
    public interface IBaseDataService<T>
    {
        public Task<List<T>> GetAllEntries();
    }
}
