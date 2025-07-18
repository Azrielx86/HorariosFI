using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorariosFI.Core.Attributes;

namespace HorariosFI.Core.Models;

public class FiTeacher
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [FiTableName("Profesor")]
    [StringLength(128)]
    public required string Name { get; set; }
    public virtual ICollection<FiGroup> Groups { get; set; } = new List<FiGroup>();

    #region MisProfesoresData

    public double? Grade { get; set; } = null;

    public double? Difficult { get; set; } = null;
    public double? Recommend { get; set; } = null;

    [StringLength(1024)]
    public string? MisProfesoresUrl { get; set; } = null;

    #endregion
}