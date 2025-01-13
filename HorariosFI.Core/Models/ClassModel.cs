using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HorariosFI.Core.Attributes;
using HorariosFI.Core.Extensions;

namespace HorariosFI.Core.Models;

[Obsolete("Will be replaced in the future", false)]
public class ClassModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    #region FiData
    
    [FiTableName("Clave")]
    public int Code { get; set; }

    [FiTableName("Gpo")]
    public double Group { get; set; }
    
    [FiTableName("Profesor")]
    [StringLength(128)]
    public string? Teacher { get; set; }

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

    #endregion

    #region MisProfesoresData

    public double? Grade { get; set; } = null;

    public double? Difficult { get; set; } = null;
    public double? Recommend { get; set; } = null;

    [StringLength(1024)]
    public string? MisProfesoresUrl { get; set; } = null;

    #endregion

    public static implicit operator ClassModel(Dictionary<string, string> data)
    {
        return CreateFromDictionary(data);
    }

    public static ClassModel CreateFromDictionary(IReadOnlyDictionary<string, string> data)
    {
        var obj = new ClassModel();
        var type = obj.GetType();
        foreach (var property in data)
        {
            var columnName = property.Key.RemoveDiacritics();
            var propertyInfo = type.GetProperty(columnName) ??
                               type.GetProperties()
                                   .FirstOrDefault(p => p
                                       .GetCustomAttributes(typeof(FiTableName), false)
                                       .Cast<FiTableName>()
                                       .Any(a => a.Name == columnName)
                                   );
            if (propertyInfo is not null)
                propertyInfo.SetValue(obj, Convert.ChangeType(property.Value, propertyInfo.PropertyType), null);
            else
                Console.WriteLine($"Error en la propiedad: {property.Key}");
        }

        return obj;
    }

    public override string ToString()
    {
        return $"{Group} - {Teacher}";
    }
}