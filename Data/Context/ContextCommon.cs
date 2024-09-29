using Data.Entity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    internal static class ContextCommon
    {
        public static void SetGuidsOnAdd(ChangeTracker ct)
        {
            ct.Entries().Where(x => x.Entity is BaseEntity && x.State == EntityState.Added)
                .Select(x => (BaseEntity)x.Entity).ToList()
                .ForEach(item =>
                item.Guid = Guid.NewGuid()
                );
        }
    }
}
