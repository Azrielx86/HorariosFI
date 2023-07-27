using DocumentFormat.OpenXml.Spreadsheet;
using HorariosFI.Core.Extensions;
using SpreadsheetLight;

using SColor = System.Drawing.Color;

namespace HorariosFI.Core;

public enum PropIndex
{
    Ignore,
    Clave,
    Group,
    Name,
    Grade,
    Difficult,
    Recommend,
    Tipo,
    Horario,
    Dias,
    Cupo,
    MPUrl,
}

public class ExcelExport
{
    private static readonly string[] HEADER = { "Clave", "Gpo", "Profesor", "Tipo", "Horario", "Dias", "Cupo" };

    public static void Export(Dictionary<string, IEnumerable<ClassModel>>? classes)
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

            document.SetCellValue(1, (int)PropIndex.Clave, "Clave");
            document.SetCellValue(1, (int)PropIndex.Group, "Grupo");
            document.SetCellValue(1, (int)PropIndex.Name, "Nombre");
            document.SetCellValue(1, (int)PropIndex.Grade, "Calificación");
            document.SetCellValue(1, (int)PropIndex.Difficult, "Dificultad");
            document.SetCellValue(1, (int)PropIndex.Recommend, "Recomendado");
            document.SetCellValue(1, (int)PropIndex.Tipo, "Tipo");
            document.SetCellValue(1, (int)PropIndex.Horario, "Horarios");
            document.SetCellValue(1, (int)PropIndex.Dias, "Dias");
            document.SetCellValue(1, (int)PropIndex.Cupo, "Cupo");
            document.SetCellValue(1, (int)PropIndex.MPUrl, "Link");

            document.SetColumnWidth((int)PropIndex.Name, 30);

            foreach (var (item, index) in classItem.Value.WithIndex())
            {
                document.SetCellValue(index + 2, (int)PropIndex.Clave, item.Clave);
                document.SetCellValue(index + 2, (int)PropIndex.Group, item.Gpo);
                document.SetCellValue(index + 2, (int)PropIndex.Name, item.Profesor);
                document.SetCellValue(index + 2, (int)PropIndex.Grade, item.Grade);
                document.SetCellValue(index + 2, (int)PropIndex.Difficult, item.Difficult);
                document.SetCellValue(index + 2, (int)PropIndex.Recommend, item.Recommend);
                document.SetCellValue(index + 2, (int)PropIndex.Tipo, item.Tipo);
                document.SetCellValue(index + 2, (int)PropIndex.Horario, item.Horario);
                document.SetCellValue(index + 2, (int)PropIndex.Cupo, item.Cupo);
                document.SetCellValue(index + 2, (int)PropIndex.Dias, item.Dias);
                document.SetCellValue(index + 2, (int)PropIndex.MPUrl, item.MisProfesoresUrl ?? "NA");

                // Grade
                if (item.Grade >= 7)
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, green);
                else if (item.Grade >= 5 && item.Grade < 7)
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, yellow);
                else
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, red);

                // Difficult
                if (item.Difficult <= 2.5)
                    document.SetCellStyle(index + 2, (int)PropIndex.Difficult, green);
                else if (item.Difficult <= 4 && item.Difficult > 2.5)
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