using DocumentFormat.OpenXml.Spreadsheet;
using SpreadsheetLight;

using SColor = System.Drawing.Color;

namespace HorariosFI.Core;

public enum PropIndex
{
    Group = 1,
    Name = 2,
    Grade = 3,
    Difficult = 4,
    Recommend = 5,
    MPUrl = 6,
}

public class ExcelExport
{
    public static void Export(Dictionary<int, IEnumerable<ClassModel>>? classes)
    {
        if (classes is null) return;
        using var document = File.Exists("FISchedules.xlsx") ? new SLDocument("FISchedules.xlsx") : new SLDocument();

        var red = document.CreateStyle();
        red.Fill.SetPattern(PatternValues.Solid, SColor.LightCoral, SColor.White);
        var yellow = document.CreateStyle();
        yellow.Fill.SetPattern(PatternValues.Solid, SColor.Khaki, SColor.White);
        var green = document.CreateStyle();
        green.Fill.SetPattern(PatternValues.Solid, SColor.PaleGreen, SColor.White);
        var gray = document.CreateStyle();
        gray.Fill.SetPattern(PatternValues.Solid, SColor.LightGray, SColor.White);

        foreach (var classItem in classes)
        {
            var wsName = $"Clase {classItem.Key}";
            if (document.GetWorksheetNames().Contains(wsName))
                continue;
            document.AddWorksheet(wsName);

            document.SetCellValue(1, (int)PropIndex.Group, "Grupo");
            document.SetCellValue(1, (int)PropIndex.Name, "Nombre");
            document.SetCellValue(1, (int)PropIndex.Grade, "Calificación");
            document.SetCellValue(1, (int)PropIndex.Difficult, "Dificultad");
            document.SetCellValue(1, (int)PropIndex.Recommend, "Recomendado");
            document.SetCellValue(1, (int)PropIndex.MPUrl, "Link");

            document.SetColumnWidth((int)PropIndex.Name, 30);

            foreach (var (item, index) in classItem.Value.WithIndex())
            {
                document.SetCellValue(index + 2, (int)PropIndex.Group, item.Group);
                document.SetCellValue(index + 2, (int)PropIndex.Name, item.Name);
                document.SetCellValue(index + 2, (int)PropIndex.Grade, item.Grade);
                document.SetCellValue(index + 2, (int)PropIndex.Difficult, item.Difficult);
                document.SetCellValue(index + 2, (int)PropIndex.Recommend, item.Recommend);
                document.SetCellValue(index + 2, (int)PropIndex.MPUrl, item.MisProfesoresUrl ?? "NA");

                // Grade
                if (item.Grade >= 7)
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, green);
                else if (item.Grade >= 5 && item.Grade < 7)
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, yellow);
                else
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, red);

                // Difficult
                if (item.Difficult >= 7)
                    document.SetCellStyle(index + 2, (int)PropIndex.Difficult, green);
                else if (item.Difficult >= 5 && item.Difficult < 7)
                    document.SetCellStyle(index + 2, (int)PropIndex.Difficult, yellow);
                else
                    document.SetCellStyle(index + 2, (int)PropIndex.Difficult, red);

                // Recommend
                if (item.Recommend >= 70)
                    document.SetCellStyle(index + 2, (int)PropIndex.Recommend, green);
                else if (item.Recommend >= 50 && item.Recommend < 70)
                    document.SetCellStyle(index + 2, (int)PropIndex.Recommend, yellow);
                else
                    document.SetCellStyle(index + 2, (int)PropIndex.Recommend, red);
            }
        }
        document.SaveAs("FISchedules.xlsx");
    }
}