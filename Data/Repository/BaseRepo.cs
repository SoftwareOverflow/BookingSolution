using Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Data.Repository
{
    /// <summary>
    /// Base class for all repositories which require signed in user
    /// </summary>
    /// <param name="factory"></param>
    internal class BaseRepo(IDbContextFactory<ApplicationDbContext> factory)
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory = factory;

        /// <summary>
        /// Execute an async method against the database context
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">The method to execute, receiving the db context & currently logged in userId as parameters</param>
        /// <returns>The result of the awaited task to be executed</returns>
        public async Task<T?> ExecuteAsync<T>(Func<ApplicationDbContext, string, Task<T?>> task) where T : class
        {
            using var context = await _factory.CreateDbContextAsync();

            var userId = context.GetCurrentUserId();
            if (userId.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Cannot find logged in user");
            }

            return await task(context, userId);
        }

        /// <summary>
        /// Execute a non-returning async method against the database context
        /// </summary>
        /// <param name="task">The method to execute, receiving the db context & currently logged in userId as parameters</param>
        public async Task ExecuteVoidAsync(Func<ApplicationDbContext, string, Task> task)
        {
            using var context = await _factory.CreateDbContextAsync();

            var userId = context.GetCurrentUserId();
            if (userId.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Cannot find logged in user");
            }

            await task(context, userId);
        }

        /// <summary>
        /// Execute a method against the database context
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="task">The method to execute, receiving the db context & currently logged in userId as parameters</param>
        /// <returns>The result of the task to be executed</returns>
        public T Execute<T>(Func<ApplicationDbContext, string, T> task)
        {
            using var context = _factory.CreateDbContext();

            var userId = context.GetCurrentUserId();
            if (userId.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Unable to create business. Cannot find logged in user");
            }

            return task(context, userId);
        }
    }
}
