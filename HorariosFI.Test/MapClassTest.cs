using HorariosFI.Core.Models;

namespace HorariosFI.Test;

public class MapClassTest
{
    [Test]
    public void TestMapTeacher()
    {
        Dictionary<string, string> testValue = [];

        testValue.Add("Clave", "119");
        testValue.Add("Gpo", "1");
        testValue.Add("Profesor", "MARGARITA CARRERA FOURNIER");
        testValue.Add("Tipo", "T");
        testValue.Add("Cupo", "35");
        testValue.Add("Vacantes", "0");
        testValue.Add("Días", "");
        testValue.Add("Salón", "");
        testValue.Add("Horario", "");

        var result = ClassModel.CreateFromDictionary(testValue);
        result.Grade = 0;
        result.Difficult = 0;
        result.Recommend = 0;
        result.MisProfesoresUrl = "";

        var type = result.GetType();

        foreach (var property in type.GetProperties())
        {
            Assert.That(property.GetValue(result), Is.Not.Null);
        }
    }
}