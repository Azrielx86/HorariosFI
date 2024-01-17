using System.Reflection;
using HorariosFI.Core.Extensions;

namespace HorariosFI.Core;

public class ClassModel
{
    // De tabla horarios
    public int Clave { get; set; }

    public double Gpo { get; set; }
    public string? Profesor { get; set; }
    public string? Tipo { get; set; }
    public string? Horario { get; set; }
    public string? Dias { get; set; }
    public int Cupo { get; set; }
    public int Vacantes { get; set; }
    public string? Salon { get; set; }

    // Para MisProfesores
    public double? Grade { get; set; } = null;

    public double? Difficult { get; set; } = null;
    public double? Recommend { get; set; } = null;
    public string? MisProfesoresUrl { get; set; } = null;

    public static ClassModel CreateFromDictionary(IReadOnlyDictionary<string, string> data)
    {
        var obj = new ClassModel();
        var type = obj.GetType();
        foreach (var property in data)
        {
            var propertyInfo = type.GetProperty(property.Key.RemoveDiacritics());
            if (propertyInfo is not null)
                propertyInfo.SetValue(obj, Convert.ChangeType(property.Value, propertyInfo.PropertyType), null);
            else
                Console.WriteLine($"Error en la propiedad: {property.Key}");
        }

        return obj;
    }
    
    public override string ToString()
    {
        return $"{Gpo} - {Profesor}";
    }
}