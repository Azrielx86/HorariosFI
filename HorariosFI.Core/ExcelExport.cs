using System.Text.RegularExpressions;
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
    MpUrl,
}

public partial class ExcelExport
{
    private const string Filename = "Horarios_FI.xlsx";

    public static void Export(int clave, string name, List<ClassModel>? classes)
    {
        if (classes is null) return;
        using var document = File.Exists(Filename) ? new SLDocument(Filename) : new SLDocument();

        var red = document.CreateStyle();
        red.Fill.SetPattern(PatternValues.Solid, SColor.LightCoral, SColor.White);
        var yellow = document.CreateStyle();
        yellow.Fill.SetPattern(PatternValues.Solid, SColor.Khaki, SColor.White);
        var green = document.CreateStyle();
        green.Fill.SetPattern(PatternValues.Solid, SColor.PaleGreen, SColor.White);
        var gray = document.CreateStyle();
        gray.Fill.SetPattern(PatternValues.Solid, SColor.LightGray, SColor.White);


        var wsName = $"{clave}-{ReplaceSpaces().Replace(name, " ")}"[..(name.Length > 25 ? 25 : name.Length)];
        if (document.GetWorksheetNames().Contains(wsName))
            return;
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
        document.SetCellValue(1, (int)PropIndex.MpUrl, "Link");

        document.SetColumnWidth((int)PropIndex.Name, 30);

        foreach (var (item, index) in classes.WithIndex())
        {
            document.SetCellValue(index + 2, (int)PropIndex.Clave, item.Clave);
            document.SetCellValue(index + 2, (int)PropIndex.Group, item.Gpo);
            document.SetCellValue(index + 2, (int)PropIndex.Name, item.Profesor);
            document.SetCellValue(index + 2, (int)PropIndex.Grade, item.Grade.ToString() ?? "NA");
            document.SetCellValue(index + 2, (int)PropIndex.Difficult, item.Difficult.ToString() ?? "NA");
            document.SetCellValue(index + 2, (int)PropIndex.Recommend, item.Recommend.ToString() ?? "NA");
            document.SetCellValue(index + 2, (int)PropIndex.Tipo, item.Tipo);
            document.SetCellValue(index + 2, (int)PropIndex.Horario, item.Horario);
            document.SetCellValue(index + 2, (int)PropIndex.Cupo, item.Cupo);
            document.SetCellValue(index + 2, (int)PropIndex.Dias, item.Dias);
            document.SetCellValue(index + 2, (int)PropIndex.MpUrl, item.MisProfesoresUrl ?? "NA");

            switch (item.Grade)
            {
                // Grade
                case >= 7:
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, green);
                    break;
                case >= 5 and < 7:
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, yellow);
                    break;
                default:
                    document.SetCellStyle(index + 2, (int)PropIndex.Grade, red);
                    break;
            }

            switch (item.Difficult)
            {
                // Difficult
                case <= 2.5:
                    document.SetCellStyle(index + 2, (int)PropIndex.Difficult, green);
                    break;
                case <= 4 and > 2.5:
                    document.SetCellStyle(index + 2, (int)PropIndex.Difficult, yellow);
                    break;
                default:
                    document.SetCellStyle(index + 2, (int)PropIndex.Difficult, red);
                    break;
            }

            switch (item.Recommend)
            {
                // Recommend
                case >= 70:
                    document.SetCellStyle(index + 2, (int)PropIndex.Recommend, green);
                    break;
                case >= 50 and < 70:
                    document.SetCellStyle(index + 2, (int)PropIndex.Recommend, yellow);
                    break;
                default:
                    document.SetCellStyle(index + 2, (int)PropIndex.Recommend, red);
                    break;
            }
        }

        document.SaveAs(Filename);
    }

    public static void ExportAll(Dictionary<string, IEnumerable<ClassModel>>? classes)
    {
        if (classes is null) return;
        using var document = File.Exists(Filename) ? new SLDocument(Filename) : new SLDocument();

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
            var wsName = $"{classItem.Key}";
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
            document.SetCellValue(1, (int)PropIndex.MpUrl, "Link");

            document.SetColumnWidth((int)PropIndex.Name, 30);

            foreach (var (item, index) in classItem.Value.WithIndex())
            {
                document.SetCellValue(index + 2, (int)PropIndex.Clave, item.Clave);
                document.SetCellValue(index + 2, (int)PropIndex.Group, item.Gpo);
                document.SetCellValue(index + 2, (int)PropIndex.Name, item.Profesor);
                document.SetCellValue(index + 2, (int)PropIndex.Grade, item.Grade.ToString() ?? "NA");
                document.SetCellValue(index + 2, (int)PropIndex.Difficult, item.Difficult.ToString() ?? "NA");
                document.SetCellValue(index + 2, (int)PropIndex.Recommend, item.Recommend.ToString() ?? "NA");
                document.SetCellValue(index + 2, (int)PropIndex.Tipo, item.Tipo);
                document.SetCellValue(index + 2, (int)PropIndex.Horario, item.Horario);
                document.SetCellValue(index + 2, (int)PropIndex.Cupo, item.Cupo);
                document.SetCellValue(index + 2, (int)PropIndex.Dias, item.Dias);
                document.SetCellValue(index + 2, (int)PropIndex.MpUrl, item.MisProfesoresUrl ?? "NA");

                switch (item.Grade)
                {
                    // Grade
                    case >= 7:
                        document.SetCellStyle(index + 2, (int)PropIndex.Grade, green);
                        break;
                    case >= 5 and < 7:
                        document.SetCellStyle(index + 2, (int)PropIndex.Grade, yellow);
                        break;
                    default:
                        document.SetCellStyle(index + 2, (int)PropIndex.Grade, red);
                        break;
                }

                switch (item.Difficult)
                {
                    // Difficult
                    case <= 2.5:
                        document.SetCellStyle(index + 2, (int)PropIndex.Difficult, green);
                        break;
                    case <= 4 and > 2.5:
                        document.SetCellStyle(index + 2, (int)PropIndex.Difficult, yellow);
                        break;
                    default:
                        document.SetCellStyle(index + 2, (int)PropIndex.Difficult, red);
                        break;
                }

                switch (item.Recommend)
                {
                    // Recommend
                    case >= 70:
                        document.SetCellStyle(index + 2, (int)PropIndex.Recommend, green);
                        break;
                    case >= 50 and < 70:
                        document.SetCellStyle(index + 2, (int)PropIndex.Recommend, yellow);
                        break;
                    default:
                        document.SetCellStyle(index + 2, (int)PropIndex.Recommend, red);
                        break;
                }
            }
        }

        document.SaveAs(Filename);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex ReplaceSpaces();
}