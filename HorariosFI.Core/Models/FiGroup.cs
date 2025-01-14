using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorariosFI.Core.Attributes;

namespace HorariosFI.Core.Models;

public class FiGroup
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [FiTableName("Gpo")]
    public int Group { get; set; }

    [FiTableName("Tipo")]
    [StringLength(8)]
    public string? Type { get; set; }

    [FiTableName("Horario")]
    [StringLength(128)]
    public string? Schedules { get; set; }

    [FiTableName("Dias")]
    [StringLength(64)]
    public string? Days { get; set; }

    [FiTableName("Cupo")]
    public int Quota { get; set; }

    [FiTableName("Vacantes")]
    public int Vacancies { get; set; }

    [FiTableName("Salon")]
    [StringLength(16)]
    public string? Classroom { get; set; }

    public int FiClassId { get; set; }
    public virtual FiClass FiClass { get; set; } = null!;

    public Guid FiTeacherId { get; set; }
    public virtual FiTeacher FiTeacher { get; set; } = null!;
}