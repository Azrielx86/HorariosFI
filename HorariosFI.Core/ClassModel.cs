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

    // Para MisProfesores
    public double Grade { get; set; }

    public double Difficult { get; set; }
    public double Recommend { get; set; }
    public string? MisProfesoresUrl { get; set; }

    public override string ToString()
    {
        return $"{Gpo} - {Profesor}";
    }
}