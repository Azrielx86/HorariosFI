namespace HorariosFI.Core.Exeptions;

public class ClassNotFoundException : Exception
{
    public override string Message => "Horarios no encontrados.";
    public override string ToString() => $"ClassNotFoundException: {Message}";
}