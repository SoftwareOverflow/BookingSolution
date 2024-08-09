using System.ComponentModel.DataAnnotations;

namespace Data.Entity
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique GUID reference to abstract the Int primary key away
        /// for potential security reasons
        /// </summary>
        public Guid Guid { get; set; } = Guid.NewGuid();

        public BaseEntity()
        {
            Guid = Guid.NewGuid();
        }
    }
}
