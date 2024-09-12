﻿using Auth.Interfaces;
using Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Data.Context
{
    internal partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IUserService userService) : DbContext(options)
    {
        private readonly IUserService UserService = userService;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            LimitBusinessControlledResults(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            SetGuidsOnAdd();

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetGuidsOnAdd();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void SetGuidsOnAdd()
        {
            ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && x.State == EntityState.Added)
                .Select(x => (BaseEntity)x.Entity).ToList()
                .ForEach(item =>
                item.Guid = Guid.NewGuid()
                );
        }

        private void LimitBusinessControlledResults(ModelBuilder modelBuilder)
        {
            // Slightly hard to read - cannot use null propogating operator in Expression.
            // Require that we have a businessUser, with a valid & matching BusinessId. Only return items related to the current business
            Expression<Func<BusinessControlledEntity, bool>> filterExpr =
                bce => bce.BusinessId == ((BusinessUsers.SingleOrDefault((x => x.UserId == UserService.GetCurrentUserId())) != null ? BusinessUsers.Single(x => x.UserId == UserService.GetCurrentUserId()).BusinessId : int.MinValue));

            foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes())
            {
                // check if current entity type is child of BaseModel
                if (mutableEntityType.ClrType.IsAssignableTo(typeof(BusinessControlledEntity)))
                {
                    // modify expression to handle correct child type
                    var parameter = Expression.Parameter(mutableEntityType.ClrType);
                    var body = ReplacingExpressionVisitor.Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
                    var lambdaExpression = Expression.Lambda(body, parameter);

                    // set filter
                    mutableEntityType.SetQueryFilter(lambdaExpression);
                }
            }
        }

        private async Task<int> GetBusinessId()
        {
            var userId = UserService.GetCurrentUserId() ?? throw new UnauthorizedAccessException("Unable to find logged in user");
            var businessUser = await BusinessUsers.SingleOrDefaultAsync(x => x.UserId == userId);

            return businessUser == null
                ? throw new InvalidOperationException("Unable to find business for current user")
                : businessUser.BusinessId;
        }
    }
}
