using Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    internal class BaseRepo(IDbContextFactory<ApplicationDbContext> factory)
    {
        private readonly IDbContextFactory<ApplicationDbContext> Factory = factory;

        /// <summary>
        /// Execute an async method against the database context
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">The async method to execute, receiving the db context as an incoming parameter</param>
        /// <returns>The result of the awaited task to be executed</returns>
        public async Task<T?> ExecuteAsync<T>(Func<ApplicationDbContext, Task<T?>> task) where T : class
        {
            using var context = await Factory.CreateDbContextAsync();
            return await task(context);
        }

        /// <summary>
        /// Execute a non-returning async method against the database context
        /// </summary>
        /// <param name="task">The async method to execute, receiving the db context as an incoming parameter</param>
        public async Task ExecuteVoidAsync(Func<ApplicationDbContext, Task> task)
        {
            using var context = await Factory.CreateDbContextAsync();
            await task(context);
        }

        /// <summary>
        /// Execute a method against the database context
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">The method to execute, receiving the db context as an incoming parameter</param>
        /// <returns>The result of the task to be executed</returns>
        public T Execute<T>(Func<ApplicationDbContext, T> task)
        {
            using var context = Factory.CreateDbContext();
            return task(context);
        }
    }
}
