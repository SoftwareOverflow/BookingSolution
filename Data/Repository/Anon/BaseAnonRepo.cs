using Microsoft.EntityFrameworkCore;

namespace Data.Repository.Anon
{
    /// <summary>
    /// Base class for all repositories which access database function anonymously (without access to a logged in user)
    /// </summary>
    /// <param name="factory"></param>
    internal class BaseAnonRepo<T1>(IDbContextFactory<T1> factory) where T1 : DbContext
    {
        private readonly IDbContextFactory<T1> _factory = factory;

        /// <summary>
        /// Execute an async method against the database context, without checking for a logged in user
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">The method to execute, receiving the db context</param>
        /// <returns>The result of the awaited task to be executed</returns>
        public async Task<T?> ExecuteAsync<T>(Func<T1, Task<T?>> task) where T : class
        {
            using var context = await _factory.CreateDbContextAsync();
            return await task(context);
        }

        /// <summary>
        /// Execute a non-returning async method against the database context, without checking for a logged in user
        /// </summary>
        /// <param name="task">The method to execute, receiving the db context</param>
        public async Task ExecuteVoidAsync(Func<T1, Task> task)
        {
            using var context = await _factory.CreateDbContextAsync();
            await task(context);
        }

        /// <summary>
        /// Execute a method against the database context
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">The method to execute, receiving the db context</param>
        /// <returns>The result of the method to be executed</returns>
        public T Execute<T>(Func<T1, T> task)
        {
            using var context = _factory.CreateDbContext();
            return task(context);
        }
    }
}
