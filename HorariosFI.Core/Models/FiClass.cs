using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorariosFI.Core.Attributes;

namespace HorariosFI.Core.Models;

public class FiClass
{
    [Key]
    [FiTableName("Clave")]
    public int Code { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;

    public virtual ICollection<FiGroup> FiGroups { get; set; } = new List<FiGroup>();
}