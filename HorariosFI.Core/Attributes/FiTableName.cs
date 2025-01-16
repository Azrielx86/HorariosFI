namespace HorariosFI.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FiTableName(string name) : Attribute
{
    public string Name { get; } = name;
}