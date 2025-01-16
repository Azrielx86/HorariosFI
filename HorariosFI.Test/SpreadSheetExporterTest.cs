using HorariosFI.Core;
using HorariosFI.Core.Models;

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
        var clases = new List<FiClassModel>()
        {
            new()
            {
                Schedules = "00:00",
                Days = "Lun, Mar",
                Difficult = null,
                Recommend = null,
                Code = 119,
                Quota = 40,
                Grade = null,
                Type = "T",
                Group = 4,
                Teacher = $"Profesor null",
                Classroom = "B550",
                MisProfesoresUrl = null,
                Vacancies = 40
            }
        };

        var exporter = new SpreadSheetExporter($"{nameof(NullInsertsTest)}.xlsx");
        exporter.Export(119, "Null Values Test", clases);
    }

    [Test]
    public void ValueRangesTest()
    {
        var clases = new List<FiClassModel>();

        for (var i = 0; i < 100; i++)
        {
            var difficult = i * 5 / 100.0;
            var grade = i * 10 / 100.0;

            var c = new FiClassModel
            {
                Schedules = "00:00",
                Days = "Lun, Mar",
                Difficult = difficult,
                Recommend = i,
                Code = 119,
                Quota = 40,
                Grade = grade,
                Type = "T",
                Group = 4,
                Teacher = $"Profesor {i}",
                Classroom = "B550",
                MisProfesoresUrl = "http://localhost",
                Vacancies = 40
            };

            clases.Add(c);
        }

        var exporter = new SpreadSheetExporter($"{nameof(ValueRangesTest)}.xlsx");
        exporter.Export(119, "Value Ranges Test", clases);
    }
}