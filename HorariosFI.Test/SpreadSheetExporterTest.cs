using HorariosFI.Core;

namespace HorariosFI.Test;

public class SpreadSheetExporterTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void NullInsertsTest()
    {
        var clases = new List<ClassModel>()
        {
            new()
            {
                Horario = "00:00",
                Dias = "Lun, Mar",
                Difficult = null,
                Recommend = null,
                Clave = 119,
                Cupo = 40,
                Grade = null,
                Tipo = "T",
                Gpo = 4,
                Profesor = $"Profesor null",
                Salon = "B550",
                MisProfesoresUrl = null,
                Vacantes = 40
            }
        };

        var exporter = new SpreadSheetExporter($"{nameof(NullInsertsTest)}.xlsx");
        exporter.Export(119, "Null Values Test", clases);
    }

    [Test]
    public void ValueRangesTest()
    {
        var clases = new List<ClassModel>();

        for (var i = 0; i < 100; i++)
        {
            var difficult = i * 5 / 100.0;
            var grade = i * 10 / 100.0;

            var c = new ClassModel
            {
                Horario = "00:00",
                Dias = "Lun, Mar",
                Difficult = difficult,
                Recommend = i,
                Clave = 119,
                Cupo = 40,
                Grade = grade,
                Tipo = "T",
                Gpo = 4,
                Profesor = $"Profesor {i}",
                Salon = "B550",
                MisProfesoresUrl = "http://localhost",
                Vacantes = 40
            };

            clases.Add(c);
        }

        var exporter = new SpreadSheetExporter($"{nameof(ValueRangesTest)}.xlsx");
        exporter.Export(119, "Value Ranges Test", clases);
    }
}