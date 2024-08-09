using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Guid { get; set; }

    }
}
