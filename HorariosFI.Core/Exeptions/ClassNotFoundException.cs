namespace HorariosFI.Core.Exeptions;

public class ClassNotFoundException(int classCode) : Exception
{
    public override string Message => $"Class not found with code ${classCode}.";
    public override string ToString() => $"ClassNotFoundException: {Message}";
}